import { AfterViewInit, Component, ElementRef, OnInit, Input, ViewChild, EventEmitter, Output, Injector, TemplateRef, HostListener } from '@angular/core';
import { ViewComponent } from './view/view.component';
import { Subject, Subscription, pipe, Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, debounceTime, finalize, switchMap, tap } from 'rxjs/operators';
import { SelectableSettings, GridComponent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { UserView } from "./models/userView";
import { ISortField } from './models/IsortField';
import * as _ from "lodash";
import { Column } from './models/column';
import { GridSearchSection } from './models/GridSearchSection';
import { ICellEventParams } from './models/ICellEventParams';
import { NotificationService } from '../services/notification.service';
import { FolderInfo } from "./models/folderInfo";
import { CrudService } from '../services/crud.service';
import { State } from '@progress/kendo-data-query';
import { CookieService } from 'ngx-cookie-service';
import { ExcelExportData } from '@progress/kendo-angular-excel-export';

@Component({
  selector: 'app-dynamic-search',
  templateUrl: './dynamic-search.component.html',
  styleUrls: ['./dynamic-search.component.scss'],
})
export class DynamicSearchComponent<ItemType extends { id: number } = any, ItemDto = any> {
  @Input() 
  searchBarLeftTemplate: TemplateRef<any>;  
  @Input()
  entityLabelPlural: string;
  @Input()
  endpoint: string;
  @Input()
  allowViewMode: boolean;
  @Output() beforeSearch: EventEmitter<UserView> = new EventEmitter();
  @Output() gridCellEvent: EventEmitter<ICellEventParams> = new EventEmitter();
  @Output() editSelectedClicked: EventEmitter<ICellEventParams> = new EventEmitter();
  @Output() confirmDeleteSelectedClicked: EventEmitter<ICellEventParams> = new EventEmitter();
  @Output() onEvent: EventEmitter<any> = new EventEmitter();
  
  // @HostListener('window:resize', ['onResize($event)'])
  protected crud: CrudService<ItemType, ItemDto>;

  public folder: FolderInfo; // holds the info for the current folder
  public userView: UserView;
  public defaultUserView: UserView;
  public newUserViewName = "";
  public saveAsViewIsPublic = false;
  public saveAsViewOpened = false;
  public deleteViewOpened = false;

  viewIsPublic = false;
  keywords = "";
  doneSearch = false;  
  gridHeight = 450;

  /** This local variable lets us use the enum in out template */
  public gridSearchSection = GridSearchSection;

  public currentSection: GridSearchSection = GridSearchSection.Basic;

  public selectedUserViewId: number;

  public loading: boolean;

  public state: State = {
      skip: 0,
      take: 100
  };

  public query: Observable<GridDataResult>;

  private stateChange = new BehaviorSubject<State>(this.state);

  public displayedColumns = ['_select', '_open', 'id', 'title'];

  public totalSelectableRecords = 0;

  //private _entityType: string; // group alias for the entityInfo

  public allSelected = false;

  public userCanAdmin: boolean;
  public userCanCreate: boolean;
  public userCanEdit: boolean;
  public userCanDelete: boolean;

  public selectedIds = [];

  fileName = 'export.xlsx';

  countText = '';

  /**This drives the columns displayed in the grid*/
  public gridColumnDefinitions: Column[];

  //showAdvanced = false;
  public showHelp = false;

  public keywordValueChangeDelay = 2000;

  public keywordStream: Subject<any> = new Subject<any>();
  public keywordSubscription: Subscription;
  public allUserViews: any = [];

  @ViewChild("quickSearch") quickSearchField: ElementRef;

  @ViewChild('userViewEditor') userViewEditor: ViewComponent;

  @ViewChild('searchContainer') searchContainer: ElementRef;
  @ViewChild('gridContainer') gridContainer: ElementRef;

  @ViewChild(GridComponent) grid: GridComponent;

  @ViewChild('input') input: ElementRef;
  userOwnsView: any;

  constructor(
    private notificationService: NotificationService,
    private cookieService: CookieService,
    injector: Injector
    ) {
      this.crud = injector.get(CrudService);
      this.keywordSubscription = this.keywordStream
      .pipe(debounceTime(this.keywordValueChangeDelay))
      .subscribe((value: any) => this.keywordsChanged());

  }

  public async setFolder(folderId: string): Promise<any> {
    const self = this;

    var folderResult = await this.crud.getFolderInfo(this.endpoint).toPromise();

    this.folder = folderResult.data;
    this.folder.id = folderId;
    this.folder.allowViews = true;
    this.userCanAdmin = this.folder.userCanAdmin;
    this.userCanCreate = this.folder.userCanCreate;
    this.userCanEdit = this.folder.userCanEdit;
    this.userCanDelete = this.folder.userCanDelete;
    this.userOwnsView = false;
    this.userView = this.folder.userView;
    this.defaultUserView =  this.folder.userView;

    var savedUserView = this.loadUserViewFromCookie();
    if (savedUserView) {
      this.userView = JSON.parse(savedUserView);
    }

    this.userViewEditor.setUserView(this.userView, this.folder);
    this.userViewEditor.showColumnProperties = true;

    if (this.userView.query.keywords && this.userView.query.keywords.length > 0 || this.userView.query.lines.length > 0) {
      this.keywordsChanged();
    }

    this.fileName = this.entityLabelPlural.replace(' ','-').toLowerCase() + '.xlsx';
    this.crud.getAllUserViews(this.endpoint).subscribe(result => {
      this.allUserViews = result.data;
    });

  }

  public async refreshSearch() {
    if (this.doneSearch) {
      await this.doSearch();
    }    
  }

  public selectableSettings(): SelectableSettings {
    if (this.userCanEdit) {
      return {
        checkboxOnly: false,
        mode: 'multiple',
        enabled: true
      }
    }
    else {
      return {
        enabled: false
      }
    }
  }

  ngOnInit() {

  }

  ngAfterViewInit() {
    // this.onResize(null);
  }

  public viewEditorSubmitted() {
    // collect edited view
    this.userView = this.userViewEditor.getUserView();
    this.doAdvancedSearch();
  }

  public columnHeaderClick(columnName: string) {
    this.sortFieldColumnHeaderClicked(columnName);
    this.doSearch();
  }

   /**
   * Put's the field at the top of the sort list. If that field is already at the top and ascending then it will change it to descending. If it's already descending then it will remove it.
   * @param fieldName
   */
  public sortFieldColumnHeaderClicked(columnName: string) {
    const sortField: ISortField = this.userView.sortFields.find(f => f.fieldName == columnName);
    if (sortField) {
      if (sortField.isAscending) {
        sortField.isAscending = false;
      } else {
        this.userView.sortFields.splice(this.userView.sortFields.indexOf(sortField), 1);
      }
    } else {
      const newSortField: ISortField = { fieldName: columnName, displayText: '', isAscending: true };
      this.userView.sortFields.unshift(newSortField);
    }
  }

  public isAscendingSort(columnName: string) {
    return (this.userView.sortFields.length > 0 && this.userView.sortFields[0].fieldName == columnName && this.userView.sortFields[0].isAscending)
  }

  public isDescendingSort(columnName: string) {
    return (this.userView.sortFields.length > 0 && this.userView.sortFields[0].fieldName == columnName && !this.userView.sortFields[0].isAscending)
  }

  public sortableIconClass(columnName: string): string {
    var topSortField = this.userViewEditor.sortableFields[0];
    if (topSortField && topSortField.fieldName == columnName) {
      if (topSortField.isAscending) {
        return "k-icon k-i-sort-asc-sm"
      } else {
        return "k-icon k-i-sort-desc-sm"
      }
    } else {
      return "";
    }
  }

  public keywordsChanged() {
    this.doSearch();
  }

  /** Get the array of column definitions to be used by the grid */
  public getSelectedGridColumns() {
    const selectedColumnNames: string[] = this.userView.selectedColumnNames;

    this.gridColumnDefinitions = [];
    
    for(const field of _.cloneDeep(this.folder.columnDefinitions.filter(function (colDef) {
      return colDef.fixed || (!colDef.filterOnly && selectedColumnNames.includes(colDef.columnName));
    }).sort((a, b) => selectedColumnNames.indexOf(a.columnName) - selectedColumnNames.indexOf(b.columnName)))) {
      this.gridColumnDefinitions.push(field);
    };

    if (this.userView.customColumnWidths) {
      for(const item of this.userView.customColumnWidths) {
        if (item.field) {
          const colDef = this.gridColumnDefinitions.find(x => x.columnAlias == item.field);
          if (colDef) {
            colDef.width = item.width;
          }
        }
      }
    }

  }

  public async doSearch(keywords: string = null) {
    let self = this;

    if (this.userView) {
      var userViewCopy = _.cloneDeep(this.userView);

      if (keywords) {
        userViewCopy.query.keywords = keywords;
      }

      this.saveUserViewToCookie();
  
      this.beforeSearch.emit(userViewCopy);
  
      this.getSelectedGridColumns();
  
      self.grid.skip = 0;

      self.grid.scrollTo({column: 0, row: 0});

      self.query = self.stateChange.pipe(
        tap(state => {
            self.loading = true;
        }),
        switchMap(state => 
          self.crud.search(
            self.folder.id,
            userViewCopy,
            state.skip,
            state.take, 
            self.endpoint)),
        tap((result) => {
          self.loading = false;
          if (result.total == 0) {
            self.countText = 'No items';
          }
          else if (result.total == 1) {
            self.countText = '1 item';
          }
          else {
            self.countText = `${result.total} items`;
          }
        })
      ); 

      this.doneSearch = true;

    }
  }

  allData = () => {
    if (this.userView) {
      var userViewCopy = _.cloneDeep(this.userView);

      this.beforeSearch.emit(userViewCopy);
  
      this.getSelectedGridColumns();
  
      return this.crud.search(
        this.folder.id,
        userViewCopy,
        0,
        0, 
        this.endpoint);
    }
  }

  public gridUserSelectionChange(selection) {
    if (selection.selectedRows && selection.selectedRows.length > 0) {
      this.grid.expandRow(selection.selectedRows[0].index);
    }
    if (selection.deselectedRows && selection.deselectedRows.length > 0) {
      this.grid.collapseRow(selection.deselectedRows[0].index);
    }
  }

  public gridCellClicked(event) {
    var dataItem = event.dataItem;
    if (dataItem.isExpanded) {
      this.grid.collapseRow(event.rowIndex);
      dataItem.isExpanded = false;
    }
    else {
      this.grid.expandRow(event.rowIndex);
      dataItem.isExpanded = true;
    }
  }

  public gridColumnResized(event) {
    var column = event[0].column;
    var newWidth = event[0].newWidth;
    this.userViewEditor.setCustomColumnWidth(column.field, newWidth);
    this.saveUserViewToCookie();
  }

  public getUserViewFromJson(json: string): UserView {
    var parseError: boolean = false;
    try {
      let userView = JSON.parse(json);
      return userView;
    } catch {
      parseError = true;
      return null;
    }
  }

  public applyKeywordFilter(value: string) {
    // we want to debounce the search so it does not run on each keypress
    this.keywords = value;
    this.keywordStream.next(value);
  }

  public doAdvancedSearch() {
    this.userView = this.userViewEditor.getUserView();

    this.closePanels();
    this.doSearch();
  }

  public closePanels() {
    this.currentSection = GridSearchSection.Basic;
  }

  public clearKeywords() {
    this.userView.query.keywords = '';
    this.doSearch();
  }

  public handleCellTemplateEvent(args: ICellEventParams) {
    this.gridCellEvent.emit(args);
  }

  public hasSelectedItems(): boolean {
    return this.allSelected || this.selectedIds.length > 0;
  }

  public selectedCount(): number {
    return this.selectedIds.length;
  }

  // view commands:

  public async saveUserView() {
    await this.crud.saveUserView(this.userView.id, this.userViewEditor.getUserView(), this.endpoint).toPromise();
    this.notificationService.success("View saved successfully");

    var viewsResult = await this.crud.getAllUserViews(this.endpoint).toPromise();
    
    this.allUserViews = viewsResult.data;
  }

  public saveUserViewAs() {
    this.newUserViewName = ""; 
    this.saveAsViewIsPublic = false;

    this.saveAsViewOpened = true;
  }

  public async saveAsViewSave() {
    var result = await this.crud.saveUserViewAs(this.newUserViewName, this.userViewEditor.getUserView(), (this.userCanAdmin && this.saveAsViewIsPublic), this.endpoint).toPromise();

    this.saveAsViewOpened = false;
    this.userView.id = result.data.userViewId;
    this.selectedUserViewId = this.userView.id;
    this.userOwnsView = true;
    this.viewIsPublic = this.userCanAdmin && this.saveAsViewIsPublic;
    var viewsResult = await this.crud.getAllUserViews(this.endpoint).toPromise();
    
    this.allUserViews = viewsResult.data;

    this.notificationService.success('View saved');
  }

  resetUserView() {
    this.keywords = '';
    this.userView = this.defaultUserView;
    this.userOwnsView = false;
    this.selectedUserViewId = null;
    this.userViewEditor.setUserView(this.userView, this.folder);
    this.userViewEditor.showColumnProperties = true;
  }

  public async deleteViewConfirm() {
    var result = await this.crud.deleteUserView(this.userView.id, this.endpoint).toPromise();

    this.deleteViewOpened = false;

    this.userView = this.folder.userView;
    this.userView.id = 0;
    this.selectedUserViewId = 0;

    var viewsResult = await this.crud.getAllUserViews(this.endpoint).toPromise();
    
    this.allUserViews = viewsResult.data;

    this.notificationService.success('View deleted');
  }

  public deleteUserView() {
    this.deleteViewOpened = true;
  }

  public closeViewEditor() {
    this.userViewEditor.setUserView(this.userView);
    this.currentSection = GridSearchSection.Basic;
  }

  public async userViewSelectionChanged() {
    if (this.selectedUserViewId && this.selectedUserViewId > 0) {
      await this.loadUserViewFromId(this.selectedUserViewId);
    }
  }

  /**
 * Using Id=0 here will fetch the defaultView
 * @param id
 */
  public async loadUserViewFromId(id: number) {
    var result = await this.crud.getUserView(id, this.endpoint).toPromise();
    this.keywords = '';
    this.userView.query.keywords = '';
    this.userView = result.data;
    this.userView.id = id;
    this.viewIsPublic = result.data.isPublic;
    this.userOwnsView = result.data.userCanEdit;
    this.userViewEditor.setUserView(this.userView, this.folder);
    this.userViewEditor.showColumnProperties = true;
  }

  public setDisplayModeToGrid() {
    //this.userSelectedGridMode = true;
    this.doSearch();
  }

  public editSelected() {
    this.editSelectedClicked.emit();
  }

  public confirmDeleteSelected() {
    this.confirmDeleteSelectedClicked.emit();
  }

  public clearSelection() {
    this.allSelected = false;
    if (!this.selectedIds) {
      this.selectedIds = [];
    }
    else {
      this.selectedIds.length = 0;
    }
  }

  public saveAsViewClose() {
    this.newUserViewName = '';
    this.saveAsViewOpened = false;
  }
  public deleteViewClose() {
    this.deleteViewOpened = false;
  }

  raiseEvent(action: string, id: number) {
    this.onEvent.emit({
      action : action,
      id: id
    });
  }

  toggleAdvancedSearch() {
    this.currentSection = (this.currentSection == this.gridSearchSection.Advanced ? this.gridSearchSection.Basic : this.gridSearchSection.Advanced);

    if (this.currentSection == this.gridSearchSection.Advanced) {
      this.userViewEditor.setUserView(this.userView, this.folder);
    }
  }

/*   onResize(event) {

    // const container = window.document.getElementsByClassName('lp-content')[0];
    const container = this.getContainer();

    console.log('grid', this.grid, this.getOffset(container).top, container.offsetTop, container.offsetHeight, this.gridContainer.nativeElement.offsetTop); 
    this.gridHeight = container.offsetHeight - 100;
    console.log('window', window);
    console.log('container', container, container.offsetHeight);
    console.log('searchContainer', this.searchContainer);
    
  }
  
  getContainer() : any {
    let element = this.searchContainer.nativeElement;
    while (element.parentElement != null) {
      console.log("testing element", element.style.top, element.offsetTop, element);
      if (element.classList.contains('lp-content')) {
        return element;
      }

      element = element.parentElement;
    }
  } */

  saveUserViewToCookie() {
    this.cookieService.set(`folder-view-${this.folder.id}`, JSON.stringify(this.userView));
  }

  loadUserViewFromCookie() {
    var cookie = this.cookieService.get(`folder-view-${this.folder.id}`);
    if (cookie) {
      return cookie.valueOf()
    }

    return null;
  }

  public pageChange(state: PageChangeEvent): void {
    this.stateChange.next(state);
  }

  private getOffset( el ) {
    var _x = 0;
    var _y = 0;
    while( el && !isNaN( el.offsetLeft ) && !isNaN( el.offsetTop ) ) {
        _x += el.offsetLeft - el.scrollLeft;
        _y += el.offsetTop - el.scrollTop;
        el = el.offsetParent;
    }
    return { top: _y, left: _x };
}
}
