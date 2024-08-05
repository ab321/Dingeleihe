import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LibraryCustomer } from '../shared/libraryCustomer';

@Component({
  selector: 'app-library-customer',
  templateUrl: './library-customer.component.html',
  styleUrls: ['./library-customer.component.css']
})
export class LibraryCustomerComponent {
  @Input() editMode: boolean = false;
  @Input() doEdit: boolean = false;
  @Input() libraryCustomer!: LibraryCustomer;
  @Output() doneEvent = new EventEmitter();
  @Output() onEditCanceled = new EventEmitter();

  editedLibraryCustomer: LibraryCustomer = {...this.libraryCustomer};

  saveThing(): void {

    this.libraryCustomer = this.editedLibraryCustomer;

    this.doEdit = false;
    this.doneEvent.emit();
  }

  cancleEdit(){
    this.doEdit = false;
    this.onEditCanceled.emit();
  }
}
