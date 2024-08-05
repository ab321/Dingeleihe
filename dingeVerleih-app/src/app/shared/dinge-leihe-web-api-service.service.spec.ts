import { TestBed } from '@angular/core/testing';

import { DingeLeiheWebApiServiceService } from './dinge-leihe-web-api-service.service';

describe('DingeLeiheWebApiServiceService', () => {
  let service: DingeLeiheWebApiServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DingeLeiheWebApiServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
