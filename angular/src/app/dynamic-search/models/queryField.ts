import { IQueryField } from './IqueryField';
import { ColumnDataType } from './columnDataType';

export class QueryField implements IQueryField {
  id: string;
  text: string;
  type: ColumnDataType;
  hasValueOptions: boolean;
}
