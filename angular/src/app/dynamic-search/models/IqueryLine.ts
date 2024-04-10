import { ColumnDataType } from './columnDataType';

export interface IQueryLine {
  field: string;
  queryOperator: string;
  value: string;
  dropdownValue?: string;
  useDropdown?: boolean;
  dataType?: ColumnDataType;
  operators?: any[];
  valueOptions?: any[]; 
}
