import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageTopbarNavComponent } from './page-topbar-nav.component';

describe('PageTopbarNavComponent', () => {
  let component: PageTopbarNavComponent;
  let fixture: ComponentFixture<PageTopbarNavComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PageTopbarNavComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageTopbarNavComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
