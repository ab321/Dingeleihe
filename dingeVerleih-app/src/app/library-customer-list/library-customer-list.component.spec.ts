import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibraryCustomerListComponent } from './library-customer-list.component';

describe('LibraryCustomerListComponent', () => {
  let component: LibraryCustomerListComponent;
  let fixture: ComponentFixture<LibraryCustomerListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LibraryCustomerListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibraryCustomerListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
