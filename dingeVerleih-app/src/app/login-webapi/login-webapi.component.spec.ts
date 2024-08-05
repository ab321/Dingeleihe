import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginWebapiComponent } from './login-webapi.component';

describe('LoginWebapiComponent', () => {
  let component: LoginWebapiComponent;
  let fixture: ComponentFixture<LoginWebapiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LoginWebapiComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginWebapiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
