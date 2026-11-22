import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { formatBig } from '../../utils/format';

@Component({
  selector: 'app-combination-display',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './combination-display.html',
  styleUrl: './combination-display.css',
})
export class CombinationDisplay {
  readonly values = input.required<number[]>();
  readonly index = input<string | null>(null);
  protected readonly format = formatBig;
}
