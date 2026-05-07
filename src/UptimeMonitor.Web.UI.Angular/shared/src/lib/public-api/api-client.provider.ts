import { Api } from "./api-client.service";

export const providePublicApiClient = (baseUrl:string) => {
    return {
        provide: Api,
        useFactory: () => {
            const api = new Api<string>({
                baseUrl: baseUrl
            });
        
            return api;
        },
        deps: []
    }
}