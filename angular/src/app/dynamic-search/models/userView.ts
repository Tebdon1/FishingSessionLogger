import { Query } from './query';
import { ISortField } from './IsortField';

export class UserView {
  constructor() { }

  id: number;
  entityType: string;
  name: string;
  selectedColumnNames: string[] = [];
  customColumnWidths: any[] = [];
  query: Query;
  sortFields: ISortField[] = [];
  isPublic: boolean;
  userCanEdit: boolean;
  gridState: any;
}
