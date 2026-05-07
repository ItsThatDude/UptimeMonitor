import { inject, Injectable } from "@angular/core";
import { Api, CreateStatusPageRequest, StatusPageConfigResponse, StatusPageSimpleDtoPagedListResponse, UpdateStatusPageRequest } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { GetStatusPageResponse } from "@uptime-monitor.web.ui.angular/shared/public-api";

@Injectable({ providedIn: 'root' })
export class StatusPageService {
    private readonly apiClient: Api<string> = inject(Api<string>);

    getStatusPages(pageSize?: number, page?: number): Promise<StatusPageSimpleDtoPagedListResponse> {
        return this.apiClient.admin.statusPagesGetList({ page, pageSize })
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch status pages');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching status pages:', error);
                throw error;
            });
    }
    
    getStatusPage(slug:string): Promise<StatusPageConfigResponse> {
        return this.apiClient.admin.statusPagesGetBySlug(slug)
        .then(response => {
            if(!response.ok) {
                throw new Error('Failed to fetch status page');
            }

            return response.data;
        }).catch(error => {
            console.error('Error fetching status page:', error);
            throw error;
        });
    }

    createStatusPage(data:CreateStatusPageRequest): Promise<GetStatusPageResponse> {
        return this.apiClient.admin.statusPagesCreate(data)
        .then(response => {
            if(!response.ok) {
                throw new Error('Failed to create status page');
            }

            return response.data;
        }).catch(error => {
            console.error('Error creating status page:', error);
            throw error;
        });
    }

    setDefault(slug:string): Promise<void> {
        return this.apiClient.admin.statusPagesSetDefault(slug)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to set default status page');
                }
            }).catch(error => {
                console.error('Error setting default status page:', error);
                throw error;
            });
    }

    updateStatusPage(slug:string, data:UpdateStatusPageRequest): Promise<GetStatusPageResponse> {
        return this.apiClient.admin.statusPagesUpdate(slug, data)
        .then(response => {
            if(!response.ok) {
                throw new Error('Failed to create status page');
            }

            return response.data;
        }).catch(error => {
            console.error('Error creating status page:', error);
            throw error;
        });
    }

    deleteStatusPage(slug:string): Promise<void> {
        return this.apiClient.admin.statusPagesDelete(slug)
        .then(response => {
            if(!response.ok) {
                throw new Error('Failed to delete status page');
            }
        }).catch(error => {
            console.error('Error deleting status page:', error);
            throw error;
        });
    }
}