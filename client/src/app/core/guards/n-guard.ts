import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { CombinationStore } from '../services/combination.store';

/** Redirects to the start screen when no n has been chosen yet. */
export const nSelectedGuard: CanActivateFn = () => {
  const store = inject(CombinationStore);
  return store.n() !== null ? true : inject(Router).createUrlTree(['/']);
};
