import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { CombinationStore, NO_MORE_MESSAGE } from './core/services/combination.store';
import { formatBig } from './shared/utils/format';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('should create the app', () => {
    expect(TestBed.createComponent(App).componentInstance).toBeTruthy();
  });

  it('formats 20! with thousands separators', () => {
    expect(formatBig('2432902008176640000')).toBe('2,432,902,008,176,640,000');
  });

  it('store: start -> next until exhausted', async () => {
    const store = TestBed.inject(CombinationStore);
    const http = TestBed.inject(HttpTestingController);

    const started = store.start(1);
    http.expectOne((r) => r.url.endsWith('/start')).flush({ totalCombinations: '1' });
    expect(await started).toBe(true);
    expect(store.total()).toBe('1');

    const nxt = store.next();
    http.expectOne((r) => r.url.endsWith('/next'))
      .flush({ serialNumber: '1', combination: [1], message: null });
    await nxt;
    expect(store.lastCombination()).toEqual([1]);
    expect(store.exhausted()).toBe(true);

    const done = store.next();
    expect(store.exhausted()).toBe(true);
    await done;
    expect(store.error()).toBe(NO_MORE_MESSAGE);
    http.verify();
  });
});


