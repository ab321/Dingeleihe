import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Lending } from '../shared/lending';
import { Thing } from '../shared/thing';
import { LibraryCustomer } from '../shared/libraryCustomer';
import { ThingRepositoryService } from '../shared/thing-repository.service';
import { LibraryCustomerRepositoryService } from '../shared/library-customer-repository.service';

@Component({
  selector: 'app-lending',
  templateUrl: './lending.component.html',
  styleUrls: ['./lending.component.css']
})
export class LendingComponent {

  @Input() editMode: boolean = true;
  @Input() doEdit: boolean = false;
  @Input() lending!: Lending;
  @Output() doneEvent = new EventEmitter();
  @Output() onEditCanceled = new EventEmitter();
  
  constructor(
    public thingRepository: ThingRepositoryService,
    public libraryCustomerRepository: LibraryCustomerRepositoryService
    ) { }

  selectedThing: Thing = new Thing();
  selectesLibraryCustomer: LibraryCustomer = new LibraryCustomer();
  selectedThingId: number = -1;
  selectedCustomerId: number = -1;
  
  editLending: Lending = {...this.lending};

  parseDate(eventDate: Event): Date{
    const dateString = (eventDate.target as HTMLInputElement).value;
    if(dateString){
      return new Date(dateString);
    }

    return new Date();
  }

  isOverdue(): boolean{
    
    if(this.lending.lendEnd.getTime() < Date.now())
      return true;

    return false;
  }

  saveLending(): void {
    this.editLending.libraryCustomer = this.selectesLibraryCustomer;
    this.editLending.thing = this.selectedThing;
    this.editLending.thingId = this.selectedThingId;
    this.editLending.libraryCustomerId = this.selectedCustomerId;
    this.lending = this.editLending;

    this.doEdit = false;
    this.doneEvent.emit();
  }

  cancleEdit(){
    this.doEdit = false;
    this.onEditCanceled.emit();
  }
}
