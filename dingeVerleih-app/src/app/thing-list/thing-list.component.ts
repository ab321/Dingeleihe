import { Component, Input } from '@angular/core';
import { ThingRepositoryService } from '../shared/thing-repository.service';
import { Thing } from '../shared/thing';

@Component({
  selector: 'app-thing-list',
  templateUrl: './thing-list.component.html',
  styleUrls: ['./thing-list.component.css']
})
export class ThingListComponent {
  constructor(public thingRepository: ThingRepositoryService) { }

  newThing: Thing = new Thing();
  doEdit: boolean = false;
  @Input() editable: boolean = true;

  onNew(){
    this.newThing = new Thing();
    this.doEdit = true;
  }

  onDelete(i: number){
    this.thingRepository.remove(i);
  }

  onDoneEvent(){
    this.doEdit = false;
    this.thingRepository.add(this.newThing);
  }

  onCancleEdit(){
    this.doEdit = false;
  }
}
