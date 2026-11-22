import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { formatBig } from '../../utils/format';

export const PAGE_SIZES = [10, 20, 50, 100];

@Component({
  selector: 'app-paginator',
  imports: [MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './paginator.html',
  styleUrl: './paginator.css',
})
export class Paginator {
  /** 0-based page number. */
  readonly page = input.required<bigint>();
  readonly totalPages = input.required<bigint>();
  readonly pageSize = input.required<number>();
  readonly pageChange = output<bigint>();
  readonly pageSizeChange = output<number>();

  protected readonly sizes = PAGE_SIZES;
  protected readonly format = formatBig;
  protected readonly first = 0n;
  protected readonly one = 1n;
  protected readonly jumpText = signal('');

  protected readonly last = computed(() => (this.totalPages() > 0n ? this.totalPages() - 1n : 0n));

  protected readonly isFirst = computed(() => this.page() === 0n);
  protected readonly isLast = computed(() => this.page() >= this.last());
  protected readonly prev = computed(() => this.page() - 1n);
  protected readonly nextPage = computed(() => this.page() + 1n);
  protected readonly currentLabel = computed(() => formatBig(this.page() + 1n));

  /** Window of up to 5 page numbers around the current page. */
  protected readonly pages = computed(() => {
    const cur = this.page();
    const last = this.last();
    let start = cur - 2n < 0n ? 0n : cur - 2n;
    let end = start + 4n > last ? last : start + 4n;
    start = end - 4n < 0n ? 0n : end - 4n;
    const out: bigint[] = [];
    for (let p = start; p <= end; p++) out.push(p);
    return out;
  });

  protected go(p: bigint): void {
    const clamped = p < 0n ? 0n : p > this.last() ? this.last() : p;
    if (clamped !== this.page()) this.pageChange.emit(clamped);
  }

  protected jump(): void {
    const text = this.jumpText().replace(/[,\s]/g, '');
    if (!/^\d+$/.test(text)) return;
    const n = BigInt(text);
    if (n < 1n) return;
    this.go(n - 1n);
    this.jumpText.set('');
  }
}

