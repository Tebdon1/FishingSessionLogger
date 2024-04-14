import { IQueryLine } from './IqueryLine';
export interface IQuery {
  keywords: string;
  searchMethod: string;
  lines: IQueryLine[];
  searchExpression: string;
}
