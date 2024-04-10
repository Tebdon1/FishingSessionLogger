import {
  Component,
  OnInit,
  AfterViewInit,
  Output,
  EventEmitter,
  ViewChild,
} from "@angular/core";
import * as _ from "lodash";
import { Column } from "../models/column";
import { UserView } from "../models/userView";
import { IColumnSelection } from "../models/columnSelection";
import { QueryComponent } from "../query/query.component";
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem
} from "@angular/cdk/drag-drop";
import { ISortField } from '../models/IsortField';
import { FolderInfo } from "../models/folderInfo";

@Component({
  selector: "app-view",
  templateUrl: "./view.component.html",
  styleUrls: ["./view.component.scss"]
})

export class ViewComponent implements OnInit, AfterViewInit {
  @Output() closed: EventEmitter<any> = new EventEmitter();
  @Output() changed: EventEmitter<any> = new EventEmitter();
  @Output() search: EventEmitter<any> = new EventEmitter();

  @ViewChild("queryEditor") queryEditor: QueryComponent; // get reference to child component which we've named #folder

  folderInfo: FolderInfo = null;

  private viewModified = false; // set if user changes config
  userOwnsView = false;
  /**This user view is the one they edit and can eventually submit. It's the one being shown in the editor**/
  public userView: UserView = new UserView();
  private loadedLastView = false;
  public showColumnProperties = false;

  /**Avaiable columns are the columns that are available to be selected, but aren't actually currently selected.*/
  availableColumns: IColumnSelection[] = [];

  /**The column selection for the bit where the user selects columns*/
  selectedColumns: IColumnSelection[] = [];

  /**Fields available to use for sorting*/
  sortableFields: ISortField[] = [];
  columnsSearchText = "";

  allUserViews: any = [];

  constructor(
  ) {}

  ngOnInit() {}

  ngAfterViewInit() {
    this.queryEditor.changed.subscribe(event => {
      this.onChanged();
    });
  }

  /** Copy the view they have edited across to the stored active view. Do this when we submit the editable view  **/
  //private saveRestoreUserView() {
  //  this.restoreUserView = _.cloneDeep(this.userView);
  //}

  /** Copy the last submitted/active view into the editable view. Do this when we want to cancel changes to the view **/
  //cancelUserViewChanges() {
  //  this.userView = _.cloneDeep(this.restoreUserView);
  //  var selectedColumns = this.userView.selectedColumnNames;
  //  var availableCols = this.getAvailableColumns(selectedColumns);
  //  this.availableColumns = availableCols;
  //}

  /**
   * When passed the array of selected columns, will return an array of available columns (the columns not yet selected from the list of Column Definitions)
   * @param selectedColumns
   */
  private getAvailableColumns(selectedColumns: string[]): IColumnSelection[] {
    return this.folderInfo.columnDefinitions.filter(function (colDef) {
      if (colDef.filterOnly || colDef.hidden || colDef.fixed || (!colDef.title || colDef.title == '')) {
        return false;
      }
      if (selectedColumns.includes(colDef.columnName)) {
        return false;
      }
      return true;
    }).map(x => <IColumnSelection>{ columnName: x.columnName, columnDisplayName: x.title });
  }


  private quickSearchChanged() {
    this.search.emit();
  }

  private mapColumnType(enumType: number): string {
    switch (enumType) {
      case 0:
        return 'string';
      case 1:
        return 'integer';
      case 2:
        return 'decimal';
      case 3:
        return 'date';
      case 4:
        return 'boolean';
      case 5:
        return 'enum';
      case 6:
        return 'collectionMultiple';
      case 7:
        return 'collectionSingle';
      default:
        return 'string';
    }
  }


  public setUserView(userView: UserView, folderInfo: FolderInfo = null) {
    this.userView = userView;
    this.userOwnsView = userView.userCanEdit;

    if (folderInfo) this.folderInfo = folderInfo;

    this.initialiseView();
  }

  /** Call this when you want to load a new userview */
  private initialiseView() {

    this.initialiseColumns();

    /** note this query editor is used for display and editing of search criteria. It is passed into userView.Query when submitted using getQuery and setQuery **/
    this.queryEditor.initialise(this.folderInfo);

    this.queryEditor.setQuery(this.userView.query);

    this.initialiseSortFields();

    //this.saveRestoreUserView();
  }

  private initialiseSortFields() {
    //console.log('initialiseSortFields');

    this.sortableFields = this.folderInfo.columnDefinitions.filter(function (column: Column) {
      return column.sortable;
    }).map(function (column: Column) {
      return {
        fieldName: column.columnName,
        displayText: column.title || column.columnName,
        isAscending: true
      };
    })

    var viewSortableFields: string[] = [];

    this.sortableFields.forEach(function (sortField: ISortField) {
      viewSortableFields.push(sortField.fieldName);
    });

    //console.log('viewSortableFields', viewSortableFields);

    this.userView.sortFields = this.userView.sortFields ? this.userView.sortFields.filter(function (field) {
      // safety first here, remove any selected fields that shouldn't be selectable
      return viewSortableFields.includes(field.fieldName);
    }) : [];

    //console.log('sortFields after filter', JSON.stringify(this.userView.sortFields));

    if (this.userView.sortFields.length == 0 || this.userView.sortFields[this.userView.sortFields.length - 1].fieldName) {
      this.addEmptySortLine();
    }

    this.doColumnSortDisplay();
  }

  private doColumnSortDisplay() {
    // not currently doing anything. Was meant for adding an indicator to the grid when a column is sorted on.

    //if (this.userView.sortFields) {
    //  var topSortField = this.userView.sortFields[0];
    //  if (topSortField) {
    //    var col = this.userView.selectedColumnNames.find(x => x.columnName == topSortField.fieldName);
    //    if (col) {
    //      col.isSortColumn = true;
    //      col.isAscendingSort = topSortField.isAscending;
    //    }
    //  }
    //}
  }

  public selectColumn(column: IColumnSelection) {
    this.availableColumns.splice(this.availableColumns.indexOf(column), 1);
    this.selectedColumns.push(column);
  }

  public removeColumn(column: IColumnSelection) {
    this.selectedColumns.splice(this.selectedColumns.indexOf(column), 1);
    this.availableColumns.push(column);
  }

  public moveColumnUp(column: IColumnSelection) {
    const index = this.selectedColumns.indexOf(column);
    const newArray: IColumnSelection[] = [];

    let i = 0;
    this.selectedColumns.forEach(column => {
      if (i < index - 1) {
        newArray.push(column);
      }
      else if (i == index - 1) {
        newArray.push(this.selectedColumns[index]);
        newArray.push(column);
      }
      else if (i > index) {
        newArray.push(column);
      }

      i++;
    });

    this.selectedColumns = newArray;
  }

  public moveColumnDown(column: IColumnSelection) {
    const index = this.selectedColumns.indexOf(column);
    const newArray: IColumnSelection[] = [];

    let i = 0;
    this.selectedColumns.forEach(column => {
      if (i < index) {
        newArray.push(column);
      }
      else if (i == index) {
        newArray.push(this.selectedColumns[index + 1]);
        newArray.push(column);
      }
      else if (i > index + 1) {
        newArray.push(column);
      }

      i++;
    });

    this.selectedColumns = newArray;
  }

  public getUserView(): UserView {
    // returns the view after extracting the query from the query editor
    this.userView.query = this.queryEditor.getQuery();
    this.userView.selectedColumnNames = this.selectedColumns.map(x => x.columnName);

    return this.userView;
  }

  private initialiseColumns() {
    // This bit here is to guard against changes to column definitions. For example, if a column is selected for a userview, then at somepoint it is changed to filterOnly.
    // That column will be in the userview definition, but seeing as the column is now filteronly, we don't want it so show up in the list of selected columns anymore.
    this.selectedColumns = [];

    for(const columnName of this.userView.selectedColumnNames) {
      const definition = this.folderInfo.columnDefinitions.find(x => x.columnName == columnName);

      if (definition && definition.title && !definition.hidden && !definition.filterOnly && !definition.fixed) {
        this.selectedColumns.push({ columnName:definition.columnName, columnDisplayName:definition.title} );
      }
    }

    this.userView.selectedColumnNames = this.selectedColumns.map(x => x.columnName);

    // add unselected columns to the available columns array. Order is kept same as entityInfo's available columns:
    this.availableColumns = this.getAvailableColumns(this.userView.selectedColumnNames);

    this.columnsSearchText = "";
  }

  public dropColumn(event: CdkDragDrop<string[]>) {
    if (event.previousContainer !== event.container) {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    } else {
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
  }

  private removeSortField(index: number) {
    this.userView.sortFields.splice(index, 1);
  }

  private sortFieldChanged(evt, field: ISortField) {
    //console.log("sortFieldChanged");
    field.fieldName = evt.value;
    this.addEmptySortLine();
  }

  /** Adds a line to the order fields UI ready to be selected */
  private addEmptySortLine() {
    this.userView.sortFields.push({ fieldName: "", displayText: "", isAscending: true });
  }

  /** Completely clear the search selected search criteria */
/*   public clear() {
    this.userView.selectedColumnNames.length = 0;
    this.userView.sortFields.length = 0;
    this.addEmptySortLine();
  }
 */
  public reset() {
    // clone view for restore
    //this.userView = _.cloneDeep(this.restoreUserView);


    this.viewModified = false;

    // note this query object is only used for display and editing of search criteria
    // it is passed into userView.Query when submissted using getQuery and setQuery
    this.queryEditor.initialise(this.folderInfo);

    this.queryEditor.setQuery(this.userView.query);

  }

  public setCustomColumnWidth(field: string, width: number) {
    if (!this.userView.customColumnWidths) {
      this.userView.customColumnWidths = [];
    }
    const existing = this.userView.customColumnWidths.find(x => x.field == field);

    if (existing) {
      existing.width = width;
    }
    else  {
      this.userView.customColumnWidths.push({field: field, width:width});
    }
  }

  onChanged() {
    this.changed.emit();
  }
}


