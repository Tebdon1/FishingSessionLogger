import { ColumnDataType } from './columnDataType';
import { IQueryLine } from './IqueryLine';
export class QueryLine implements IQueryLine {
  constructor() {
    this.dataType = ColumnDataType.String;
      this.field = "";
  }
  field: string;
  queryOperator: string;
  value: string;
  dropdownValue?: string;
  useDropdown?: boolean;
  dataType?: ColumnDataType;
  operators?: any[];
  valueOptions?: any[];
}
