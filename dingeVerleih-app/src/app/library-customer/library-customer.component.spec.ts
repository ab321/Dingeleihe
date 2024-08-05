import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibraryCustomerComponent } from './library-customer.component';

describe('LibraryCustomerComponent', () => {
  let component: LibraryCustomerComponent;
  let fixture: ComponentFixture<LibraryCustomerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LibraryCustomerComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibraryCustomerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
