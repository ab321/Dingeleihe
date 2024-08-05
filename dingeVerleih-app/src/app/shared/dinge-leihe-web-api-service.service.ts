import { Injectable } from '@angular/core';
import { HttpClient , HttpHeaders } from '@angular/common/http';
import { Thing } from './thing';
import { LibraryCustomer } from './libraryCustomer';
import { Lending } from './lending';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DingeLeiheWebApiServiceService {

  private readonly headers = new HttpHeaders () . set ('Accept','application/json') ;
  private readonly baseUrl = 'https://localhost:5555/';
  private readonly lendingsAll = 'lendings/all';
  private readonly thingsAll = 'things/all';
  private readonly librarycustomersAll = 'librarycustomers/all ';
  private readonly loginPath = 'security/login';

  private token: string = "";
  private isLoggedIn: boolean = false;

  constructor(private http: HttpClient) { }

  public login (u : string , p : string ) {

      return this.http.post<string>( this.baseUrl + this.loginPath,
        { "UserName": u , "Password": p },
        { headers: this.headers })
          .subscribe({
            next: t => {
              this.token = "Bearer " + t;
              this.isLoggedIn = true ;
              return of(true);
            },
            error: e => {
              console.error(e);
              this.token = "";
              this.isLoggedIn = false;
              return of(false);
            }
      });
  }

  public IsUserLoggedIn() {
    return this.isLoggedIn;
  }

  public logout() {
    this.token = "";
    this.isLoggedIn = false;
  }

  private makeHeader(): HttpHeaders {
    if(this.isLoggedIn)
      return this.headers.set('Authorization', this.token!);
    else
      return this.headers;
  }

  public getAllThings(): Observable<Thing[]>{
    return this.http.get<Thing[]>(this.baseUrl + this.thingsAll, {headers: this.makeHeader()})
  }

  public getAllLibraryCustomers(): Observable<LibraryCustomer[]>{
    return this.http.get<LibraryCustomer[]>(this.baseUrl + this.librarycustomersAll, {headers: this.makeHeader()})
  }

  public getAllLendings(): Observable<Lending[]>{
    return this.http.get<Lending[]>(this.baseUrl + this.lendingsAll, {headers: this.makeHeader()})
  }

}
