import { Api } from "./api-client.service";

export const provideAdminApiClient = (baseUrl:string) => {
    return {
        provide: Api,
        useFactory: () => {
            const api = new Api<string>({
                baseUrl: baseUrl,
                securityWorker: () => ({ credentials: 'include' })
            });
        
            return api;
        },
        deps: []
    }
}