import { Component, EventEmitter, Injector, Input, OnInit, Output, TemplateRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ValidationErrors } from '@angular/forms';
import { finalize, tap } from 'rxjs/operators';
import { DynamicSearchComponent } from '../dynamic-search/dynamic-search.component';
import { CrudService } from '../services/crud.service';
import { NotificationService } from '../services/notification.service';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { UserView } from '../dynamic-search/models/userView';
import * as _ from 'lodash';
import { UtilsService } from '../services/utils.service';

@Component({
  selector: 'app-dynamic-crud',
  templateUrl: './dynamic-crud.component.html',
  styleUrls: ['./dynamic-crud.component.scss']
})
export class DynamicCrudComponent<ItemType extends { id: number } = any, ItemDto = any> implements OnInit {
  @ViewChild('search') search: DynamicSearchComponent;
  @Input()
  entityName: string;
  @Input()
  endpoint: string;
  @Input()
  entityView: string;
  @Input()
  entityLabel: string;
  @Input()
  entityLabelPlural: string;
  @Input()
  headerTemplate: TemplateRef<any>;
  @Input()
  formTemplate: TemplateRef<any>;
  @Input()
  belowFormTemplate: TemplateRef<any>;
  @Input() 
  searchBarLeftTemplate: TemplateRef<any>;  
  @Input() 
  afterSaveTemplate: TemplateRef<any>;  
  @Input() 
  readonlyModeButtonTemplate: TemplateRef<any>;  
  @Input() 
  editModeButtonTemplate: TemplateRef<any>;  
  @Input() 
  saveAndCloseCaption: string = "Save and Close";  
  @Input()
  hasCustomSave: boolean;
  @Input()
  allowViewMode: boolean;

  @Input() 
  buildFormCallback: (fb: FormBuilder, selected: ItemType) => FormGroup;
  @Input() 
  onViewCallback: () => void;
  @Input() 
  onAddCallback: () => void;
  @Input() 
  onEditCallback: () => void;
  @Input() 
  onDeleteCallback: () => void;
  @Input() 
  hideSaveButton: boolean = false;

  @Output() beforeBuildForm: EventEmitter<any> = new EventEmitter();
  @Output() itemLoaded: EventEmitter<any> = new EventEmitter();
  @Output() beforeSave: EventEmitter<BeforeSaveArgs> = new EventEmitter();
  @Output() customSave: EventEmitter<CustomSaveArgs> = new EventEmitter();
  @Output() afterSave: EventEmitter<any> = new EventEmitter();
  
  @Output() beforeSearch: EventEmitter<UserView> = new EventEmitter();
  @Output() beforeAdd: EventEmitter<ItemType> = new EventEmitter();

  protected crud: CrudService<ItemType, ItemDto>;

  public displayMode = "search";
  readOnly = false;
  // selectedId = 0;
  selected = {} as ItemType;
  public form : FormGroup;
  busy = false;
  formErrors = "";
  error = false;
  errors = [];

  editModeFromReadonly = false;

  constructor(
    public fb: FormBuilder,
    injector: Injector,
    private notificationService: NotificationService,
    private confirmationService: ConfirmationService,
    private utils: UtilsService
  ) { 
    this.crud = injector.get(CrudService);
  }

  ngOnInit(): void {
  }

  ngAfterViewInit() {
    this.search.setFolder(this.entityName);
  }

  userCanEdit() {
    return this.search.userCanEdit;
  }

  userCanAdmin() {
    return this.search.userCanAdmin;
  }

  public viewItem(id: number) {
    this.crud.getById(id, this.endpoint).subscribe((response: ItemType) => {
      this.selected = response;
      this.selected.id = id;
      this.readOnly = true;
      this.editModeFromReadonly = false;
      if (this.onViewCallback) {
        this.onViewCallback();
      }
      this.openEditor();
    });
  }

  public editItem(id: number) {
    this.editModeFromReadonly = false;
    this.crud.getById(id, this.endpoint).subscribe((response: ItemType) => {
      this.selected = response;
      this.selected.id = id;
      this.readOnly = false;
      if (this.onEditCallback) {
        this.onEditCallback();
      }
      this.openEditor();
    });
  }

  public addItem(item = {} as ItemType) {
    this.selected = item;
    this.readOnly = false;
    if (this.onAddCallback) {
      this.onAddCallback();
    }
    this.beforeAdd.emit(this.selected);
    this.openEditor();
  }

  edit() {
    this.editItem(this.selected.id);
    this.editModeFromReadonly = true;
  }

  onEvent(event) {
    switch(event.action) {
      case 'add':
        this.addItem();
        break;
      case 'view':
        this.viewItem(event.id);
        break;
      case 'edit':
        this.editItem(event.id);
        break;
      case 'delete':
        const options: Partial<Confirmation.Options> = {
          hideCancelBtn: false,
          hideYesBtn: false,
          dismissible: false,
          cancelText: 'Cancel',
          yesText: 'Delete',
        };

        this.confirmationService
          .warn('The item will be deleted', 'Are you sure?', options)
          .subscribe((status: Confirmation.Status) => {
            if (status == 'confirm') {
              this.crud.delete(event.id, this.endpoint).subscribe(() => {
                if (this.onDeleteCallback) {
                  this.onDeleteCallback();
                }

                this.notificationService.success("Item deleted");
                this.selected = {} as ItemType;
                this.search.refreshSearch();
              }); 
            }
          });
          break;
      }
  }

  ngOnDestroy() {}

  openEditor() {
    this.beforeBuildForm.emit(this.selected);
    this.form = this.buildForm();
    this.formErrors="";
    // mark all as touched  so we get required fields highlighted
    setTimeout(() => {
      this.form.markAllAsTouched();
      this.form.updateValueAndValidity();
      this.displayMode = "edit";
      this.itemLoaded.emit(this.selected);
    }, 500);
  }

  closeEditor() {
    this.displayMode = "search";
  }

  buildForm(): FormGroup {

    var r =  this.buildFormCallback(this.fb, this.selected);
    return r;
  }

  async save() : Promise<boolean> {
    const beforeSaveArgs : BeforeSaveArgs = new BeforeSaveArgs();
    this.beforeSave.emit(beforeSaveArgs);

    if (beforeSaveArgs.customSave) {
      this.customSave.emit();
      return true;
    }

    return new Promise(async (resolve, reject) => {
      if (!this.form.valid) {
        this.getFormValidationErrors();
        
        resolve(false);
      }
      else {
        this.busy = true;
        this.errors = [];
        this.error = false;

        window.scroll(0,0);

        if (this.selected.id) {
          this.crud.update({ ...this.getValue() }, this.selected.id, this.endpoint)
          .pipe(
            tap((result) => {
              if (!result.error) {
                this.selected = result.value;
                this.afterSave.emit(true);
              }
              else {
                this.error = true;
                if (!result.errors || result.errors.length == 0) {
                  this.errors = ['There was an error when saving'];      
                }
                else {
                  this.errors = result.errors 
                }
                reject("Save failed");
              }
            }),
            finalize(() => {
              this.busy = false;
              this.form.markAsPristine();
              this.form.markAllAsTouched();
              resolve(true);
            })
          )
          .subscribe(() => {
            if (!this.error) {
              this.notificationService.success("Item saved");
            }
            else {
              this.notificationService.error("Save failed");
            }
          },
          error => {
            this.notificationService.error("Save failed");
            reject("Save failed");
          });
        }
        else {
          this.crud.create({ ...this.getValue() }, this.endpoint)
          .pipe(
            tap((result) => {
              if (!result.error) {
                this.selected = result.value;
                this.afterSave.emit(true);
              }
              else {
                this.error = true;
                if (!result.errors || result.errors.length == 0) {
                  this.errors = ['There was an error when saving'];      
                }
                else {
                  this.errors = result.errors 
                }
                reject("Save failed");
              }
            }),
            finalize(() => {
              this.busy = false;
              this.form.markAsPristine();
              this.form.markAsTouched();
              resolve(true);
            })
          )
          .subscribe(() => {
            if (!this.error) {
              this.notificationService.success("Item saved");
            }
            else {
              this.notificationService.error("Save failed");
            }
          },
          error => {
            reject("Save failed");
            this.notificationService.error("Save failed");
          });
        }
      }
    });
  }

  getValue() {
    var value = this.form.getRawValue();

    const iterateDateReplacement = (obj) => {
      Object.keys(obj).forEach(key => {
  
        if (obj[key] instanceof Date) {
          obj[key] = this.utils.dateToIsoString(obj[key]);
        }
        else if (typeof obj[key] === 'object' && obj[key] !== null) {
          iterateDateReplacement(obj[key])
        }
      })
    }

    iterateDateReplacement(value);

    return value;
  }

  async saveAndClose() {
    const beforeSaveArgs : BeforeSaveArgs = new BeforeSaveArgs();
    this.beforeSave.emit(beforeSaveArgs);

    if (beforeSaveArgs.customSave) {
      const customSaveArgs : CustomSaveArgs = _.extend(new CustomSaveArgs(), {close: true});
      this.customSave.emit(customSaveArgs);
      return;
    }
    else {
      try {
        const saved = await this.save();
  
        if (saved) {
          this.displayAfterSaveAndClose();
        }
      }
      catch {
  
      }
    }

  }

  cancel() {
    if (this.editModeFromReadonly) {
      this.viewItem(this.selected.id);
    }
    else {
      this.closeEditor();
    }
  }

  beforeSearchHandler(userView: UserView) {
    this.beforeSearch.emit(userView);
  }

  displayAfterSaveAndClose() {
    if (this.afterSaveTemplate == null) {
      if (this.editModeFromReadonly) {
        this.viewItem(this.selected.id);
      }
      else {
        this.showSearch();
      }
    }
    else {
      this.showSubmitted();
    }
  }

  showSearch() {
    this.search.doSearch();
    this.displayMode = 'search';
  }

  showSubmitted() {
    this.itemLoaded.emit(this.selected);
    this.displayMode = 'submitted';
    window.scroll(0,0);
  }

  getFormValidationErrors() {
    this.formErrors = "";
    for (const key of Object.keys(this.form.controls)) {
      const controlErrors: ValidationErrors = this.form.get(key).errors;
      if (controlErrors != null) {
        Object.keys(controlErrors).forEach(keyError => {
          this.formErrors += '<li>' + key + ' ' + keyError + '</li>';
          console.log('Key: ' + key + ', error: ' + keyError + ', value: ', controlErrors[keyError]);
        });
      }
    };
    if (this.formErrors.length > 0) {
      this.formErrors = '<ul>' + this.formErrors + '</ul>';
    }
  }


}

export class BeforeSaveArgs {
  public customSave: boolean; 
}

export class CustomSaveArgs {
  public close: boolean; 
}



