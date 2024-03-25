import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaitComponent } from './bait.component';

describe('BaitComponent', () => {
  let component: BaitComponent;
  let fixture: ComponentFixture<BaitComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BaitComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BaitComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
