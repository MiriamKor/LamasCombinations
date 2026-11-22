import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CombinationStore } from '../../core/services/combination.store';

@Component({
  selector: 'app-start-screen',
  imports: [FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './start-screen.html',
})
export class StartScreen {
  protected readonly store = inject(CombinationStore);
  private readonly router = inject(Router);

  protected readonly value = signal<number | null>(null);
  protected readonly invalid = signal(false);

  protected async start(): Promise<void> {
    const n = Number(this.value());
    if (!Number.isInteger(n) || n < 1 || n > 20) {
      this.invalid.set(true);
      return;
    }
    this.invalid.set(false);
    if (await this.store.start(n)) {
      await this.router.navigate(['/generator']);
    }
  }
}
