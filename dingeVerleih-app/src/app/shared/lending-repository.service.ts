import { Injectable } from '@angular/core';
import { Lending } from './lending';
import { DingeLeiheWebApiServiceService } from './dinge-leihe-web-api-service.service';
import { LibraryCustomerRepositoryService } from './library-customer-repository.service';
import { ThingRepositoryService } from './thing-repository.service';

@Injectable({
  providedIn: 'root'
})
export class LendingRepositoryService {

  public lendings: Lending[] = [];

  constructor(
    private readonly webApiService: DingeLeiheWebApiServiceService,
    private readonly libraryCustomerRepo: LibraryCustomerRepositoryService,
    private readonly thingRepo: ThingRepositoryService
    ) { 
    this.webApiService.getAllLendings().
    subscribe({ 
      next: (lendings) => {
        lendings.forEach(lending => {
          lending.libraryCustomer = this.libraryCustomerRepo.libraryCustomers.find(c => c.id == lending.libraryCustomerId)!;
          lending.thing = this.thingRepo.things.find(t => t.id == lending.thingId)!;
          lending.lendBegin = new Date((lending.lendBegin as unknown) as string);
          lending.lendEnd = new Date((lending.lendEnd as unknown) as string);
        });

      this.lendings = lendings;
    },
    error: e => {
      console.log(e);
    
    }});
  }

  getAll(): Lending[] {
    return this.lendings;
  }

  add(lending: Lending): void {
    if(lending.id > 0){
      let editedLending: Lending = this.lendings.find(l => l.id === lending.id)!;
      editedLending.libraryCustomer = lending.libraryCustomer;
      editedLending.thing = lending.thing;
      editedLending.lendBegin = lending.lendBegin;
      editedLending.lendEnd = lending.lendEnd;

      return;
    }

    let id: number = 1;

    if(this.lendings.length > 0){
      id = this.lendings[this.lendings.length - 1].id! + 1;
    }

    lending.id = id;
    this.lendings.push(lending);
  }

  remove(i: number): void {
    let index = this.lendings.findIndex(l => l.id === i);
    this.lendings.splice(index, 1);
  }

  update(existingLending: Lending, updatedLending: Lending): void {
    const index = this.lendings.indexOf(existingLending);
    if (index !== -1) {
      this.lendings[index] = updatedLending;
    }
  }
}
