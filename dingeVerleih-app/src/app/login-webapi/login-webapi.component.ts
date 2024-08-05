import { Component } from '@angular/core';
import { DingeLeiheWebApiServiceService } from '../shared/dinge-leihe-web-api-service.service';

@Component({
  selector: 'app-login-webapi',
  templateUrl: './login-webapi.component.html',
  styleUrls: ['./login-webapi.component.css']
})
export class LoginWebapiComponent {
  username: string = "";
  password: string = "";

  constructor(public dingeLeiheWebApi: DingeLeiheWebApiServiceService) { }

  login(): void {
    this.dingeLeiheWebApi.login(this.username, this.password);
  }

  logout(): void {
    this.dingeLeiheWebApi.logout();
  }
}
