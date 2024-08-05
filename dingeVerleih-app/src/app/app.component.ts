import { Component } from '@angular/core';
import { Thing } from './shared/thing';
import { LibraryCustomer } from './shared/libraryCustomer';
import { Lending } from './shared/lending';
import { DingeLeiheWebApiServiceService } from './shared/dinge-leihe-web-api-service.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'dingeVerleih-app';

  constructor(public dingeLeiheWebApi: DingeLeiheWebApiServiceService){}

  /*thing: Thing = new Thing(0, 'Test', 'Test', 'Test', 'Test');
  secondThing: Thing = new Thing(1, 'Test2', 'Test2', 'Test2', 'Test2');

  libraryCustomer: LibraryCustomer = new LibraryCustomer(0, 'Max Mustermann', 'maxmustermann@gmail.com');
  secondLibraryCustomer: LibraryCustomer = new LibraryCustomer(1, 'Luka cvijic', 'hondacivic@besteauto.com');*/
}
