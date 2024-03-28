import { ColumnDataType } from './columnDataType';

export class Column {
  constructor() { }

  title: string;
  filterable: boolean;
  sortable: boolean;
  /**The name of the column. This should match the name of the DB field for use in the query**/
  columnName: string;
  width: number;
  columnAlias: string;
  dataType: ColumnDataType;
  hidden: boolean;
  filterOnly: boolean;
  fixed: boolean;
  valueOptionsUrl: string;
  isSortColumn: boolean;
  isAscendingSort: boolean;
  format: string;

  /**Returns a value to indicate if the column is usable for the grid */
  public isGridColumn(): boolean {
    // returns undefined for some reason? Don't know why.
    return this.title && !this.filterOnly && !this.fixed;
  }

  public isSelectableColumn(): boolean {
    return this.title && !this.filterOnly && !this.fixed;
  }

}
