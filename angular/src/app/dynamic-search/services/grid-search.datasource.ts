import { CollectionViewer, DataSource } from "@angular/cdk/collections";
import { Observable } from "rxjs";
import { GridSearchItem } from "../models/gridSearchItem";
import { GridSearchResults } from '../models/documentResults';
import { BehaviorSubject } from "rxjs";
import { catchError, finalize } from "rxjs/operators";
import { UserView } from "../models/userView";
import { GridDataResult } from '@progress/kendo-angular-grid';
import { of } from "rxjs";
import { CrudService } from "src/app/services/crud.service";

export class GridSearchDataSource implements DataSource<GridSearchItem> {

  private searchGridRecordsSubject = new BehaviorSubject<GridSearchItem[]>([]);

  private loadingSubject = new BehaviorSubject<boolean>(false);

  private totalRecordsSubject = new BehaviorSubject<number>(0);
  private totalSelectableRecordsSubject = new BehaviorSubject<number>(0);

  public loading$ = this.loadingSubject.asObservable();

  public gridData: GridDataResult;

  constructor(private crudService: CrudService) {
  }

  loadItems(
    folderId: string,
    userView: UserView,
    endpoint: string
  ) {
    this.loadingSubject.next(true);
    this.crudService.search(
      folderId,
      userView,
      endpoint)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
    ).subscribe(results => {
      this.totalRecordsSubject.next((<GridSearchResults>results).totalRecords);
      this.totalSelectableRecordsSubject.next((<GridSearchResults>results).totalSelectableRecords);
      this.searchGridRecordsSubject.next((<GridSearchResults>results).results);
      this.gridData = { data: (<GridSearchResults>results).results, total: (<GridSearchResults>results).totalRecords};
      });
  }

  connect(collectionViewer: CollectionViewer): Observable<GridSearchItem[]> {
    return this.searchGridRecordsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.totalRecordsSubject.complete();
    this.totalSelectableRecordsSubject.complete();
    this.searchGridRecordsSubject.complete();
    this.loadingSubject.complete();
  }

  totalRecords(): Observable<number> {
    return this.totalRecordsSubject.asObservable();
  }

  totalSelectableRecords(): Observable<number> {
    return this.totalSelectableRecordsSubject.asObservable();
  }
}

