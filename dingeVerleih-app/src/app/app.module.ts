import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { ThingComponent } from './thing/thing.component';
import { LibraryCustomerComponent } from './library-customer/library-customer.component';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import { LendingComponent } from './lending/lending.component';
import { ThingListComponent } from './thing-list/thing-list.component';
import { LibraryCustomerListComponent } from './library-customer-list/library-customer-list.component';
import { LendingListComponent } from './lending-list/lending-list.component';
import { RouterModule, Routes } from '@angular/router';
import { OverviewComponent } from './overview/overview.component';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { LoginWebapiComponent } from './login-webapi/login-webapi.component';
import { LoginGuardService } from './shared/login-guard.service';
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";

const appRoutes: Routes = [
  { path: '', redirectTo: 'overview', pathMatch: 'full'},
  { path: 'login', component: LoginWebapiComponent},
  { path: 'overview', component: OverviewComponent, canActivate: [LoginGuardService] },
  { path: 'Lendings', component: LendingListComponent, canActivate: [LoginGuardService]  },
  { path: 'Things', component: ThingListComponent, canActivate: [LoginGuardService]  },
  { path: 'Library Customers', component: LibraryCustomerListComponent, canActivate: [LoginGuardService]  }
  ];

@NgModule({
  declarations: [
    AppComponent,
    ThingComponent,
    LibraryCustomerComponent,
    LendingComponent,
    ThingListComponent,
    LibraryCustomerListComponent,
    LendingListComponent,
    OverviewComponent,
    LoginWebapiComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(appRoutes),
    BrowserAnimationsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
