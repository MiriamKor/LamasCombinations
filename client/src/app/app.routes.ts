import { Routes } from '@angular/router';
import { nSelectedGuard } from './core/guards/n-guard';
import { StartScreen } from './features/start-screen/start-screen';

export const routes: Routes = [
  { path: '', component: StartScreen },
  {
    path: 'generator',
    loadComponent: () =>
      import('./features/generator-view/generator-view').then((m) => m.GeneratorView),
    canActivate: [nSelectedGuard],
  },
  {
    path: 'all',
    loadComponent: () =>
      import('./features/all-combinations-view/all-combinations-view').then(
        (m) => m.AllCombinationsView,
      ),
    canActivate: [nSelectedGuard],
  },
  { path: '**', redirectTo: '' },
];
