import { Component, Input } from '@angular/core';
import { LibraryCustomerRepositoryService } from '../shared/library-customer-repository.service';
import { LibraryCustomer } from '../shared/libraryCustomer';

@Component({
  selector: 'app-library-customer-list',
  templateUrl: './library-customer-list.component.html',
  styleUrls: ['./library-customer-list.component.css']
})
export class LibraryCustomerListComponent {

  constructor(public libraryCustomerRepository: LibraryCustomerRepositoryService) { }

  newLibraryCustomer: LibraryCustomer = new LibraryCustomer();
  doEdit: boolean = false;
  @Input() editable: boolean = true;

  onNew(){
    this.newLibraryCustomer = new LibraryCustomer();
    this.doEdit = true;
  }

  onDelete(i: number){
    this.libraryCustomerRepository.remove(i);
  }

  onDoneEvent(){
    this.doEdit = false;
    this.libraryCustomerRepository.add(this.newLibraryCustomer);
  }

  onCancleEdit(){
    this.doEdit = false;
  }

}
