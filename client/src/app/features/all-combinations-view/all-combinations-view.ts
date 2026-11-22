import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CombinationApiService } from '../../core/services/combination-api.service';
import { CombinationStore, GENERIC_ERROR_MESSAGE, NO_MORE_MESSAGE } from '../../core/services/combination.store';
import { formatBig } from '../../shared/utils/format';
import { CombinationDto } from '../../core/models/models';
import { CombinationDisplay } from '../../shared/components/combination-display/combination-display';
import { Paginator } from '../../shared/components/paginator/paginator';

/**
 * Shows the range [lastIndex+1 .. total] page by page. Pages are relative to
 * that base, so the first page always starts right after the last combination
 * shown in the generator view.
 */
@Component({
  selector: 'app-all-combinations-view',
  imports: [CombinationDisplay, Paginator, MatCardModule, MatButtonModule, MatDividerModule, MatProgressBarModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './all-combinations-view.html',
  styleUrl: './all-combinations-view.css',
})
export class AllCombinationsView implements OnInit {
  private readonly store = inject(CombinationStore);
  private readonly api = inject(CombinationApiService);
  private readonly router = inject(Router);

  private readonly totalAll = BigInt(this.store.total());
  /** Combinations already shown; pages cover only the remaining ones, as the server does. */
  private readonly base = BigInt(this.store.lastIndex());
  private readonly totalBig = this.totalAll - this.base;

  protected readonly pageSize = signal(20);
  protected readonly page = signal(0n);
  protected readonly items = signal<CombinationDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  /** Combinations remaining after the last one on the current page. */
  private readonly remaining = computed(() => {
    const items = this.items();
    if (items.length === 0) return this.totalBig;
    const left = this.totalAll - BigInt(items[items.length - 1].index);
    return left > 0n ? left : 0n;
  });

  protected readonly totalText = computed(() => formatBig(this.store.total()));
  protected readonly remainingText = computed(() => formatBig(this.remaining()));
  protected readonly totalPages = computed(() => {
    if (this.totalBig <= 0n) return 0n;
    const size = BigInt(this.pageSize());
    return (this.totalBig + size - 1n) / size;
  });

  protected readonly hasPages = computed(() => this.totalPages() > 0n);

  ngOnInit(): void {
    if (this.totalBig <= 0n) {
      this.error.set(NO_MORE_MESSAGE);
    }
    // The first page is loaded only when the user asks for it.
  }

  protected readonly firstPage = 0n;

  protected async load(page: bigint): Promise<void> {
    const n = this.store.n();
    if (n === null) return;
    this.loading.set(true);
    this.error.set(null);
    try {
      const res = await firstValueFrom(this.api.all(page, this.pageSize()));
      this.items.set(res.items);
      this.page.set(page);
    } catch {
      this.error.set(GENERIC_ERROR_MESSAGE);
    } finally {
      this.loading.set(false);
    }
  }

  protected changeSize(size: number): void {
    // Keep the first item of the current page visible after the size change.
    const firstOffset = this.page() * BigInt(this.pageSize());
    this.pageSize.set(size);
    void this.load(firstOffset / BigInt(size));
  }

  protected async back(): Promise<void> {
    // The server remembers the last item of the last loaded page; resume after it.
    if (this.items().length > 0) await this.store.syncBack();
    this.router.navigate(['/generator']);
  }
}

