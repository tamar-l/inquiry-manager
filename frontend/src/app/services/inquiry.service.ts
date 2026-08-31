import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Inquiry, InquiryQueryParams, InquirySummary, PagedResult, UpdateStatusRequest } from '../models/inquiry.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InquiryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getInquiries(params: InquiryQueryParams): Observable<PagedResult<Inquiry>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '')
        httpParams = httpParams.set(key, String(value));
    });
    return this.http.get<PagedResult<Inquiry>>(this.baseUrl, { params: httpParams });
  }

  getSummary(): Observable<InquirySummary> {
    return this.http.get<InquirySummary>(`${this.baseUrl}/summary`);
  }

  updateStatus(id: number, request: UpdateStatusRequest): Observable<Inquiry> {
    return this.http.patch<Inquiry>(`${this.baseUrl}/${id}/status`, request);
  }
}
