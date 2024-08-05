import { Injectable } from '@angular/core';
import { Thing } from './thing';
import { DingeLeiheWebApiServiceService } from './dinge-leihe-web-api-service.service';

@Injectable({
  providedIn: 'root'
})
export class ThingRepositoryService {

  public things: Thing[] = [ ];

  constructor(private apiClient: DingeLeiheWebApiServiceService){
    this.apiClient.getAllThings()
      .subscribe({
        next: (data) => {
          this.things = data;
        },
        error: (err) => {
          console.error(err)
        }});
  }

  add(thing: Thing): void {
    if(thing.id > 0){
      let editedThing: Thing = this.things.find(l => l.id === thing.id)!;
      editedThing.shortName = thing.shortName;
      editedThing.description = thing.description;
      editedThing.category = thing.category;
      editedThing.shelf = thing.shelf;

      return;
    }

    let id: number = 1;

    if(this.things.length > 0){
      id = this.things[this.things.length - 1].id! + 1;
    }

    thing.id = id;
    this.things.push(thing);
  }

  remove(i: number): void {
    let index = this.things.findIndex(l => l.id === i);
    this.things.splice(index, 1);
  }
}
