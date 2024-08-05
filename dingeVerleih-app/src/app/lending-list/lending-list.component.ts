import { Component, Input } from '@angular/core';
import { LendingRepositoryService } from '../shared/lending-repository.service';
import { ThingRepositoryService } from '../shared/thing-repository.service';
import { LibraryCustomerRepositoryService } from '../shared/library-customer-repository.service';
import { Lending } from '../shared/lending';
import { Thing } from '../shared/thing';
import { LibraryCustomer } from '../shared/libraryCustomer';
import { ActivatedRoute } from '@angular/router';


@Component({
  selector: 'app-lending-list',
  templateUrl: './lending-list.component.html',
  styleUrls: ['./lending-list.component.css']
})
export class LendingListComponent {

  constructor(
    public lendingRepository: LendingRepositoryService,
    public thingRepository: ThingRepositoryService,
    public libraryCustomerRepository: LibraryCustomerRepositoryService
    ) { }

    newThing: Thing = new Thing();
    newLibraryCustomer: LibraryCustomer = new LibraryCustomer();

    newLending: Lending = new Lending();
    doEdit: boolean = false;
    @Input() editable: boolean = true;

    onNew(){
      this.newLending = new Lending();
      this.doEdit = true;
    }

    onDelete(i: number){
      this.lendingRepository.remove(i);
    }

    onDoneEvent(){
      this.doEdit = false;
      this.lendingRepository.add(this.newLending);
    }

    onCancleEdit(){
      this.doEdit = false;
    }
}
