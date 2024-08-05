import { Injectable } from '@angular/core';
import { LibraryCustomer } from './libraryCustomer';
import { DingeLeiheWebApiServiceService } from './dinge-leihe-web-api-service.service';

@Injectable({
  providedIn: 'root'
})
export class LibraryCustomerRepositoryService {

  public libraryCustomers: LibraryCustomer[] = [];

  constructor(private apiClient: DingeLeiheWebApiServiceService) { 
    this.apiClient.getAllLibraryCustomers()
    .subscribe({ 
      next: (customers) => {
      this.libraryCustomers = customers;
    },
    error: (error) => {
      console.log(error);
    }});
  }

  add(libraryCustomer: LibraryCustomer): void {
    if(libraryCustomer.id > 0){
      let editedLibraryCustomer: LibraryCustomer = this.libraryCustomers.find(l => l.id == libraryCustomer.id)!;
      editedLibraryCustomer.firstName = libraryCustomer.firstName;
      editedLibraryCustomer.lastName = libraryCustomer.lastName;
      editedLibraryCustomer.contact = libraryCustomer.contact;

      return;
    }

    let id: number = 1;

    if(this.libraryCustomers.length > 0){
      id = this.libraryCustomers[this.libraryCustomers.length - 1].id! + 1;
    }

    libraryCustomer.id = id;
    this.libraryCustomers.push(libraryCustomer);
  }

  remove(i: number): void {
    let index = this.libraryCustomers.findIndex(l => l.id == index);
    this.libraryCustomers.splice(index, 1);
  }

}
