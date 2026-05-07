import { Api } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { ConfigService } from "../services/config.service";
import { inject } from "@angular/core";

export const provideAdminApiClient = () => {
    return {
        provide: Api,
        useFactory: () => {
            const configService = inject(ConfigService);
            const baseUrl = configService.apiBaseUrl;

            const api = new Api<string>({
                baseUrl: baseUrl,
                securityWorker: () => ({ credentials: 'include' })
            });
        
            return api;
        },
        deps: [ConfigService]
    }
}