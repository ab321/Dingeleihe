import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Thing } from '../shared/thing';

@Component({
  selector: 'app-thing',
  templateUrl: './thing.component.html',
  styleUrls: ['./thing.component.css']
})
export class ThingComponent {

  @Input() editMode: boolean = false;
  @Input() doEdit: boolean = false;
  @Input() thing!: Thing;
  @Output() doneEvent = new EventEmitter();
  @Output() onEditCanceled = new EventEmitter();

  editThing: Thing = {...this.thing};

  saveThing(): void {
    this.thing = this.editThing;
    this.doEdit = false;
    this.doneEvent.emit();
  } 

  cancleEdit(){
    this.doEdit = false;
    this.onEditCanceled.emit();
  }
}
