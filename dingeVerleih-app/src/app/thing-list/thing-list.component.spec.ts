import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThingListComponent } from './thing-list.component';

describe('ThingListComponent', () => {
  let component: ThingListComponent;
  let fixture: ComponentFixture<ThingListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ThingListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ThingListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
