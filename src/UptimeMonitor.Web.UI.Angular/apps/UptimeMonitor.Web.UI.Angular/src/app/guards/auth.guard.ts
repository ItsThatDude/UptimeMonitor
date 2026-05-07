import { inject } from "@angular/core";
import { CanActivateFn } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { filter, map, take } from "rxjs";
import { toObservable } from '@angular/core/rxjs-interop';

export const authGuard: CanActivateFn = (_, state) => {
    const authService = inject(AuthService);

    // Trigger the fetch if needed
    authService.fetchUserInfo();

    // Wait until not loading
    return toObservable(authService.isLoading).pipe(
        filter(loading => loading === false), // wait for loading to finish
        take(1),
        map(() => {
            if (authService.isAuthenticated()) {
                return true;
            } else {
                document.location.href = '/auth/login?returnUrl=' + state.url;
                return false;
            }
        })
    );
};