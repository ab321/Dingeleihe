import { TestBed } from '@angular/core/testing';

import { ThingRepositoryService } from './thing-repository.service';

describe('ThingService', () => {
  let service: ThingRepositoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ThingRepositoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
