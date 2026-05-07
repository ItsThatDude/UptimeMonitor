// auth.service.ts
import { computed, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';

export interface ClaimValue {
    type: string
    value: string
}

export interface UserInfo {
    isAuthenticated: boolean
    nameClaimType: string
    roleClaimType: string
    claims: ClaimValue[]
}

const unauthenticated = { isAuthenticated: false } as UserInfo;

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly userInfoUrl = '/api/user';

    private _userInfo = signal<UserInfo>(unauthenticated);
    private _isLoading = signal<boolean>(false);
    private _hasFetched = false;

    readonly isLoading = this._isLoading.asReadonly();
    readonly userInfo = this._userInfo.asReadonly();
    readonly isAuthenticated = computed(() => this.userInfo()?.isAuthenticated ?? false);
    readonly userName = computed(() => {
        const userInfo = this.userInfo();
        return userInfo.claims.find(c => c.type == userInfo.nameClaimType)?.value ?? "";
    })

    constructor(private http: HttpClient) { }

    fetchUserInfo(): void {
        if (this._hasFetched) return;
        this._hasFetched = true;
        this._isLoading.set(true);

        this.http.get<UserInfo>(this.userInfoUrl).pipe(
            tap(user => this._userInfo.set(user)),
            catchError(err => {
                console.error('Error fetching user info', err);
                this._userInfo.set(unauthenticated);
                return of(unauthenticated);
            }),
            tap(() => this._isLoading.set(false))
        ).subscribe();
    }

    logout(): void {
        document.location.href = '/auth/logout?returnUrl=' + '/';
    }
}