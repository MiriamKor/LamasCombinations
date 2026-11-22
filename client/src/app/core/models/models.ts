/*
 * Server contract (CombinationsAPI, session cookie based - the server keeps the state):
 * big numbers travel as strings because 20! exceeds Number.MAX_SAFE_INTEGER.
 *
 *   POST /api/combinations/start?n=3
 *       -> { totalCombinations: "6" }
 *   GET  /api/combinations/next
 *       -> { serialNumber: "2", combination: [1,3,2], message: null }
 *          (message = "No more combinations." and no serialNumber when exhausted)
 *   GET  /api/combinations/all?pageSize=10&page=1      (page is 1-based)
 *       -> { combinations: [...], totalCombinations, currentPage, totalPages, pageSize, message }
 *          Pages start at the first combination not yet shown by "next".
 *   GET  /api/combinations/back
 *       -> last displayed combination; "next" then continues right after it.
 *   POST /api/combinations/reset
 */
export interface StartResponse {
  totalCombinations: string;
}

export interface CombinationResponse {
  serialNumber: string | null;
  combination: number[];
  message: string | null;
}

export interface PageApiResponse {
  combinations: CombinationResponse[];
  totalCombinations: string;
  currentPage: string;
  totalPages: string;
  pageSize: number;
  message: string | null;
}

/** Client-side shape used by the views. */
export interface CombinationDto {
  index: string;
  values: number[];
}

export interface PageResponse {
  total: string;
  items: CombinationDto[];
}

export type ViewMode = 'start' | 'generator' | 'all';
