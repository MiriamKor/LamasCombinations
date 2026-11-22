import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  CombinationDto,
  CombinationResponse,
  PageApiResponse,
  PageResponse,
  StartResponse,
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class CombinationApiService {
  private readonly http = inject(HttpClient);

  /** Starts (or restarts) the session state for n. Returns the total as a string. */
  start(n: number): Observable<string> {
    return this.http
      .post<StartResponse>(`${API_BASE_URL}/start`, null, { params: new HttpParams().set('n', n) })
      .pipe(map((res) => res.totalCombinations));
  }

  /** Next combination (dto null when there are no more) plus the server message. */
  next(): Observable<{ dto: CombinationDto | null; message: string | null }> {
    return this.http
      .get<CombinationResponse>(`${API_BASE_URL}/next`)
      .pipe(map((res) => ({ dto: this.toDto(res), message: res.message ?? null })));
  }

  reset(): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}/reset`, null);
  }

  /** Last displayed combination; the server then continues "next" right after it. */
  back(): Observable<CombinationDto | null> {
    return this.http
      .get<CombinationResponse>(`${API_BASE_URL}/back`)
      .pipe(map((res) => this.toDto(res)));
  }

  /** @param page 0-based page (the server is 1-based). */
  all(page: bigint, size: number): Observable<PageResponse> {
    const params = new HttpParams().set('pageSize', size).set('page', (page + 1n).toString());
    return this.http.get<PageApiResponse>(`${API_BASE_URL}/all`, { params }).pipe(
      map((res) => ({
        total: res.totalCombinations,
        items: res.combinations.map((c) => this.toDto(c)!).filter(Boolean),
      })),
    );
  }

  private toDto(res: CombinationResponse): CombinationDto | null {
    if (res.serialNumber === null || res.serialNumber === undefined) return null;
    return { index: res.serialNumber, values: res.combination };
  }
}
