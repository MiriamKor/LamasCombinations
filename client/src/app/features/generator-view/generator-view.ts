import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { CombinationStore } from '../../core/services/combination.store';
import { formatBig } from '../../shared/utils/format';
import { CombinationDisplay } from '../../shared/components/combination-display/combination-display';

@Component({
  selector: 'app-generator-view',
  imports: [CombinationDisplay, MatCardModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './generator-view.html',
})
export class GeneratorView {
  protected readonly store = inject(CombinationStore);
  private readonly router = inject(Router);
  protected readonly totalText = computed(() => formatBig(this.store.total()));

  protected showAll(): void {
    this.router.navigate(['/all']);
  }

  protected async newGame(): Promise<void> {
    await this.store.clear();
    await this.router.navigate(['/']);
  }
}
