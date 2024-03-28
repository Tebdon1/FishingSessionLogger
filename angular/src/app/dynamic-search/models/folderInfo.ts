import { Column } from "./column";
import { ISortField } from "./IsortField";
import { UserView } from "./userView";

export class FolderInfo {
  id: string;
  entityType: string;
  title: string = "";
  userCanAdmin: boolean;
  userCanCreate: boolean;
  userCanEdit: boolean;
  userCanView: boolean;
  userCanDelete: boolean;
  userView: UserView;
  columnDefinitions: Column[];
  defaultColumnNames: string[];
  defaultSortFields: ISortField[];
  allowViews: boolean;
}
