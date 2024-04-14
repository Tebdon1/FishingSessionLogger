import { IQuery } from './Iquery';
import { ISortField } from './IsortField';
export class Query implements IQuery {
  keywords: string;
  searchMethod: string;
  filterSQL?: string;
  filterLines: any[];
  lines: any[];
  searchExpression: string;
  sortFields: ISortField[];

  constructor() {
    this.filterLines = [];
    this.lines = [];
    this.sortFields = [];
  }
}
