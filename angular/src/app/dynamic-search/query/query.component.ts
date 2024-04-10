import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Query } from '../models/query';
import { QueryLine } from '../models/queryLine';
import { QueryField } from '../models/queryField';
import { FolderInfo } from '../models/folderInfo';
import { Column } from '../models/column';
import { ColumnDataType } from '../models/columnDataType';
import { LookupService } from "../services/lookup.service";
import * as _ from "lodash";

@Component({
  selector: 'app-query',
  templateUrl: './query.component.html',
  styleUrls: ['./query.component.scss']
})
export class QueryComponent implements OnInit {
  @Output() changed: EventEmitter<any> = new EventEmitter();
  @Output() search: EventEmitter<any> = new EventEmitter();

  loading = false;
  standaloneMode = false;
  folderInfo: FolderInfo = null;
  queryFields: QueryField[] = [];
  query: Query = new Query();
  test = '';

  fieldDefaultItem = {value: 0, text: 'Select field...'};

  constructor(
    private lookupService: LookupService
  ) {}

  /** This local variable lets us use the enum in out template */
  columnDataType = ColumnDataType;

  ngOnInit() {}

  removeLine(index: number) {
    this.query.lines.splice(index, 1);
    this.addLineAuto();
    this.onChanged();
  }

  onChanged() {
    this.changed.emit();
  }

  fieldChanged(e, line: QueryLine) {
    line.field = e.id;
    line.value = null;
    line.queryOperator = null;
    line.dropdownValue = null;
    this.setupQueryLineForField(line);
    this.onChanged();
    this.addLineAuto();
  }

  operatorChanged(e, line: QueryLine) {
    line.queryOperator = e.value;
    this.onChanged();
  }

  dropdownValueChanged(e, line: QueryLine) {
    line.dropdownValue = e.value;
    this.onChanged();
  }

  addLineAuto() {
    if (
      this.query.lines.length === 0 ||
      (this.query.lines[this.query.lines.length - 1].field)
    ) {
      const line = new QueryLine();
      this.setupQueryLineForField(line);
      this.query.lines.push(line);
    }
  }

  getFieldByName(fieldName: string) {
    for (let i = 0; i < this.queryFields.length; i++) {
      if (this.queryFields[i] && this.queryFields[i].id === fieldName) {
        return this.queryFields[i];
      }
    }
    return null;
  }

  public getQuery(): Query {
    var lines: any[] = this.query.lines.map(function(line) {
      if (line.field) {
        var theLine = new QueryLine();

        return {
          field: line.field,
          queryOperator: line.queryOperator,
          value: line.useDropdown
            ? line.dropdownValue
            : line.dataType === ColumnDataType.Date
              ? line.value
              : line.value
          };
      } else {
        return undefined;
      }
    });

    return _.extend(new Query(), {
      searchMethod: this.query.searchMethod,
      keywords: this.query.keywords,
      searchExpression: this.query.searchExpression,
      lines: lines,
      sortFields: this.query.sortFields
    });
  }

  initialise(folderInfo: FolderInfo) {
    const module = this;
    this.folderInfo = folderInfo;
    this.queryFields = this.folderInfo.columnDefinitions.map(
      function(column: Column) {
        return column.filterable && column.title && column.title.length > 2
          ? {
              id: column.columnAlias,
              text: column.title,
              type: column.dataType,
              hasValueOptions: column.valueOptionsUrl ? true : false
            }
          : undefined;
      }
    ).sort(function(a, b) {
      if (a.text > b.text) {
        return 1;
      } else {
        return -1;
      }
    });

    this.queryFields = this.queryFields.filter(function( element ) {
      return element !== undefined;
    });
  }

  setQuery(sourceQuery: Query) {
    this.loading = true;
    this.query = new Query();

    // clear existing lines
    this.query.lines = [];

    if (sourceQuery) {
      this.query.keywords = sourceQuery.keywords;
      this.query.searchMethod = sourceQuery.searchMethod;
      this.query.searchExpression = sourceQuery.searchExpression;
  
        // add query lines from source lines {field, queryOperator, value}
      for (let i = 0; i < sourceQuery.lines.length; i++) {
        const existingLine = sourceQuery.lines[i];
        if (existingLine && existingLine.field) {
          const newLine = new QueryLine();
          this.query.lines.push(newLine);
          newLine.queryOperator = existingLine.queryOperator;
          const fieldInfo: QueryField = this.getFieldByName(existingLine.field);
          if (fieldInfo) {
            newLine.field = existingLine.field;
            newLine.queryOperator = existingLine.queryOperator;
            newLine.value = existingLine.value;
            newLine.dropdownValue = existingLine.value;
            this.setupQueryLineForField(newLine);
          } else {
            //console.log('Field ' + existingLine.field + ' not found');
          }
        }
      }
    }

    // add a new line
    if (this.query.lines.length === 0) {
      // add quick search line
      const newLine = new QueryLine();
      this.query.lines.push(newLine);
    } else {
      // add line for next query entry if we need to
      this.addLineAuto();
    }

    this.loading = false;
    this.searchTypeChanged();
  }

  searchTypeChanged() {
    if (this.loading) {
        return true;
    }

    this.addLineAuto();

    let e = '';
    switch (this.query.searchMethod) {
        case 'all':
            for (let i = 0; i < this.query.lines.length; i++) {
                const line = this.query.lines[i];
                if (line.field) {
                    if (e.length > 0) {
                        e += ' AND ';
                    }
                    e += (i + 1);
                }
            }
            this.query.searchExpression = e;
            break;
        case 'any':
          for (let i = 0; i < this.query.lines.length; i++) {
                const line = this.query.lines[i];
                if (line.field) {
                    if (e.length > 0) {
                        e += ' OR ';
                    }
                    e += (i + 1);
                }
            }
            this.query.searchExpression = e;
            break;
        case 'advanced':
            break;
    }

    this.onChanged();

    return true; // the checking of the checkbox can proceed
}

  getOperators(field: QueryField) {
    if (!field) {
      return [];
    }
    if (field.hasValueOptions) {
      if (field.type === ColumnDataType.CollectionMultiple) {
        return [
          {
            id: 'contains',
            text: 'includes'
          },
          {
            id: 'notcontains',
            text: 'does not include'
          },
          {
            id: 'blank',
            text: 'is blank'
          },
          {
            id: 'notblank',
            text: 'is not blank'
          }
        ];
      } else {
        return [
          {
            id: 'equalto',
            text: 'matches'
          },
          {
            id: 'notequalto',
            text: 'does not match'
          },
          {
            id: 'blank',
            text: 'is blank'
          },
          {
            id: 'notblank',
            text: 'is not blank'
          }
        ];
      }
    } else {
      switch (field.type) {
        case ColumnDataType.Enum:
          return [
            {
              id: 'equalto',
              text: 'equal to'
            },
            {
              id: 'notequalto',
              text: 'not equal to'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
        case ColumnDataType.Integer:
          return [
            {
              id: 'equalto',
              text: 'exactly matches'
            },
            {
              id: 'greaterthan',
              text: 'is greater than'
            },
            {
              id: 'lessthan',
              text: 'is less than'
            },
            {
              id: 'notequalto',
              text: 'does not match'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
        case ColumnDataType.Decimal:
          return [
            {
              id: 'equalto',
              text: 'exactly matches'
            },
            {
              id: 'greaterthan',
              text: 'is greater than'
            },
            {
              id: 'lessthan',
              text: 'is less than'
            },
            {
              id: 'notequalto',
              text: 'does not match'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
        case ColumnDataType.Date:
          return [
            {
              id: 'equalto',
              text: 'exactly matches'
            },
            {
              id: 'greaterthan',
              text: 'is after'
            },
            {
              id: 'lessthan',
              text: 'is before'
            },
            {
              id: 'notequalto',
              text: 'does not match'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
        case ColumnDataType.Boolean:
          return [
            {
              id: 'true',
              text: 'is true'
            },
            {
              id: 'false',
              text: 'is false'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
        case ColumnDataType.String:
        default:
          return [
            {
              id: 'equalto',
              text: 'exactly matches'
            },
            {
              id: 'contains',
              text: 'has text'
            },
            {
              id: 'notcontains',
              text: 'doesn\'t have text'
            },
            {
              id: 'notequalto',
              text: 'does not match'
            },
            {
              id: 'blank',
              text: 'is blank'
            },
            {
              id: 'notblank',
              text: 'is not blank'
            }
          ];
      }
    }
  }

  setupQueryLineForField(line: QueryLine) {
    // field changed on a query line
    // hide options until ready
    line.operators = null;
    line.useDropdown = false;
    line.valueOptions = [];

    const fieldInfo = this.getFieldByName(line.field);
    if (!fieldInfo) {
      //console.log("No fieldInfo found for " + line.field, line);
    }

    if (fieldInfo) {
      line.operators = this.getOperators(fieldInfo);
      line.useDropdown = fieldInfo.hasValueOptions;
      line.dataType = fieldInfo.type;

      if (fieldInfo.hasValueOptions) {
        const colDef = this.folderInfo.columnDefinitions.find(x => x.columnAlias == fieldInfo.id);

        if (colDef && colDef.valueOptionsUrl) {
          // get options from server
          this.lookupService.getLookupData(colDef.valueOptionsUrl.replace('/lookup', '')).subscribe(result => {
            line.valueOptions  = result.data;

            // convert numeric values to strings
            for(const option of line.valueOptions) {
              if (option.value && option.value.toString) {
                option.value = option.value.toString();
              }
            }
          });
        }
      } else {
        return null;
      }
    }
  }
}
