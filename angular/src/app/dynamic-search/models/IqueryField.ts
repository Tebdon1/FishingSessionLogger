import { ColumnDataType } from './columnDataType';

export interface IQueryField {
  id: string;
  text: string;
  type: ColumnDataType;
  hasValueOptions: boolean;
}
