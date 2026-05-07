import { Api } from "@uptime-monitor.web.ui.angular/shared/public-api";
import { ConfigService } from "../services/config.service";
import { inject } from "@angular/core";

export const providePublicApiClient = () => {
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