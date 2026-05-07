// config.service.ts
import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, firstValueFrom, of, tap } from 'rxjs';

export interface AppConfig {
    production: boolean;
    apiBaseUrl: string;
}

export interface SystemSettings {
    defaultStatusPageSlug: string
}

const notconfigured = {
    defaultStatusPageSlug: ""
} as SystemSettings;

@Injectable({ providedIn: 'root' })
export class ConfigService {
    private readonly settingsUrl = '/api/config';

    private _appConfig: AppConfig = { production: false, apiBaseUrl: '' };
    private _settings = signal<SystemSettings>(notconfigured);
    private _isLoading = signal<boolean>(false);
    private _hasFetched = false;

    readonly isLoading = this._isLoading.asReadonly();
    readonly settings = this._settings.asReadonly();

    constructor(private http: HttpClient) { }

    fetchConfig(): void {
        if (this._hasFetched) return;
        this._hasFetched = true;
        this._isLoading.set(true);

        this.http.get<SystemSettings>(this.settingsUrl).pipe(
            tap(settings => this._settings.set(settings)),
            catchError(err => {
                console.error('Error fetching configuration', err);
                this._settings.set(notconfigured);
                return of(notconfigured);
            }),
            tap(() => this._isLoading.set(false))
        ).subscribe();
    }

    public loadAppConfig(): Promise<AppConfig> {
        this._appConfig.apiBaseUrl = window.location.protocol + "//" + window.location.host;
        return firstValueFrom(this.http.get<AppConfig>('./config.json')).then(
        (config) => {
            this._appConfig.production = config.production;
            if(config.apiBaseUrl != null && config.apiBaseUrl != "") {
                this._appConfig.apiBaseUrl = config.apiBaseUrl;
            }
            return this._appConfig;
        }
        );
    }

    // Getter for the API URL
    public get apiBaseUrl(): string {
        return this._appConfig.apiBaseUrl;
    }
}