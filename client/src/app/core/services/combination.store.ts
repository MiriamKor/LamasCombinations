import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CombinationApiService } from './combination-api.service';
import { CombinationDto, ViewMode } from '../models/models';

export const NO_MORE_MESSAGE = 'אין עוד צירופים';
export const GENERIC_ERROR_MESSAGE = 'אירעה שגיאה בתקשורת עם השרת';

@Injectable({ providedIn: 'root' })
export class CombinationStore {
  private readonly api = inject(CombinationApiService);

  readonly n = signal<number | null>(null);
  readonly total = signal('0');
  /** Index (1-based, string) of the last displayed combination; '0' = none yet. */
  readonly lastIndex = signal('0');
  readonly lastCombination = signal<number[] | null>(null);
  readonly mode = signal<ViewMode>('start');
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  private history: CombinationDto[] = [];
  private readonly pos = signal(-1);
  readonly canGoBack = computed(() => this.pos() > 0);

  readonly exhausted = computed(
    () => this.n() !== null && BigInt(this.lastIndex()) >= BigInt(this.total()),
  );

  async start(n: number): Promise<boolean> {
    this.error.set(null);
    this.loading.set(true);
    try {
      const total = await firstValueFrom(this.api.start(n));
      this.n.set(n);
      this.total.set(total);
      this.resetProgress();
      this.mode.set('generator');
      return true;
    } catch (e) {
      this.error.set(this.messageFor(e));
      return false;
    } finally {
      this.loading.set(false);
    }
  }

  /** Tells the server where the paged view stopped so "next" continues after it. */
  async syncBack(): Promise<void> {
    try {
      const dto = await firstValueFrom(this.api.back());
      if (dto) this.setLast(dto);
    } catch (e) {
      this.error.set(this.messageFor(e));
    }
  }

  /** Back to the start screen. */
  async clear(): Promise<void> {
    try {
      await firstValueFrom(this.api.reset());
    } catch {
      // Server state is overwritten by the next start anyway.
    }
    this.n.set(null);
    this.total.set('0');
    this.resetProgress();
    this.error.set(null);
    this.mode.set('start');
  }

  /** Shows the previous combination (client-side history). */
  back(): void {
    const pos = this.pos();
    if (pos <= 0) return;
    this.error.set(null);
    this.pos.set(pos - 1);
    this.show(this.history[pos - 1]);
  }

  async next(): Promise<void> {
    if (this.n() === null) return;
    // Stepped back earlier: move forward through the history before asking the server.
    if (this.pos() < this.history.length - 1) {
      this.error.set(null);
      this.pos.update((p) => p + 1);
      this.show(this.history[this.pos()]);
      return;
    }
    if (this.exhausted()) {
      this.error.set(NO_MORE_MESSAGE);
      return;
    }
    this.error.set(null);
    this.loading.set(true);
    try {
      const { dto, message } = await firstValueFrom(this.api.next());
      if (!dto) {
        this.error.set(message || NO_MORE_MESSAGE);
        return;
      }
      this.setLast(dto);
    } catch (e) {
      this.error.set(this.messageFor(e));
    } finally {
      this.loading.set(false);
    }
  }

  /** Records a combination as the last one displayed (also used when leaving the paged view). */
  setLast(dto: CombinationDto): void {
    const last = this.history[this.history.length - 1];
    if (!last || last.index !== dto.index) this.history.push(dto);
    this.pos.set(this.history.length - 1);
    this.show(dto);
  }

  private show(dto: CombinationDto): void {
    this.lastIndex.set(dto.index);
    this.lastCombination.set(dto.values);
  }

  private resetProgress(): void {
    this.history = [];
    this.pos.set(-1);
    this.lastIndex.set('0');
    this.lastCombination.set(null);
  }

  private messageFor(e: unknown): string {
    if (e instanceof HttpErrorResponse) {
      if (e.status === 404) return NO_MORE_MESSAGE;
      // Server lost the session state (e.g. expired): the user has to start again.
      if (e.status === 409) return 'ההפעלה פגה, יש להתחיל מחדש';
      if (e.status === 400 && typeof e.error?.detail === 'string') return e.error.detail;
    }
    return GENERIC_ERROR_MESSAGE;
  }
}
