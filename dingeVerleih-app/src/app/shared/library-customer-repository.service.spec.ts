import { TestBed } from '@angular/core/testing';

import { LibraryCustomerRepositoryService } from './library-customer-repository.service';

describe('LibraryCustomerService', () => {
  let service: LibraryCustomerRepositoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LibraryCustomerRepositoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
