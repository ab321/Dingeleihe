import { TestBed } from '@angular/core/testing';

import { LendingRepositoryService } from './lending-repository.service';

describe('LendingService', () => {
  let service: LendingRepositoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LendingRepositoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
