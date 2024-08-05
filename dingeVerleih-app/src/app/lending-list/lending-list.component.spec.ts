import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LendingListComponent } from './lending-list.component';

describe('LendingListComponent', () => {
  let component: LendingListComponent;
  let fixture: ComponentFixture<LendingListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LendingListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LendingListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
