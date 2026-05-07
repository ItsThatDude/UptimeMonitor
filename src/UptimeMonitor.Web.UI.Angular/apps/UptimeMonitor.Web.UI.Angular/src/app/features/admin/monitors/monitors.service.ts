import { inject, Injectable } from "@angular/core";
import { Api, CreateMonitorConfigRequest, GetMonitorStatisticsResponse, MonitorConfigurationResponse, MonitorEventDtoPagedListResponse, MonitorSettingsSchemaDto, MonitorSimpleDtoPagedListResponse, MonitorTypeDto, UpdateMonitorConfigRequest } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { GetMonitorHeartbeatsResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";

@Injectable({ providedIn: 'root' })
export class MonitorService {
    private readonly apiClient: Api<string> = inject(Api<string>);

    getStatistics(): Promise<GetMonitorStatisticsResponse> {
        return this.apiClient.admin.adminGetMonitorStatistics()
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor stats');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor stats:', error);
                throw error;
            });
    }

    getMonitor(id:number): Promise<MonitorConfigurationResponse> {
        return this.apiClient.admin.adminGetMonitor(id)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor:', error);
                throw error;
            });
    }

    getEventsForMonitor(id:number, pageSize?:number, page?:number): Promise<MonitorEventDtoPagedListResponse> {
        return this.apiClient.admin.adminGetMonitorEvents(id, { pageSize: pageSize, page: page })
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor events');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor events:', error);
                throw error;
            });
    }

    getChartDataForMonitor(id:number): Promise<GetMonitorHeartbeatsResponse> {
        return this.apiClient.admin.adminGetChartDataForMonitor(id)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor metrics');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor metrics:', error);
                throw error;
            });
    }

    getChartDataForMonitors(monitorIds:number[]): Promise<GetMonitorHeartbeatsResponse[]> {
        return this.apiClient.admin.adminGetChartDataForMonitors({ ids: monitorIds })
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor metrics');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor metrics:', error);
                throw error;
            });
    }

    getChartDataForAllMonitors(): Promise<GetMonitorHeartbeatsResponse[]> {
        return this.apiClient.admin.adminGetChartDataAllMonitors()
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor metrics');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor metrics:', error);
                throw error;
            });
    }

    getMonitors(pageSize?:number, page?:number): Promise<MonitorSimpleDtoPagedListResponse> {
        return this.apiClient.admin.adminGetMonitors({ pageSize: pageSize, page: page })
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitors');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitors:', error);
                throw error;
            });
    }

    getEvents(page?: number, pageSize?: number): Promise<MonitorEventDtoPagedListResponse> {
        return this.apiClient.admin.adminGetAllMonitorEvents({ page: page, pageSize: pageSize })
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor events');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor events:', error);
                throw error;
            });
    }

    getMonitorTypes(): Promise<MonitorTypeDto[]> {
        return this.apiClient.admin.adminGetMonitorTypes()
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor types');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor types:', error);
                throw error;
            });
    }

    getMonitorTypeSettings(): Promise<MonitorSettingsSchemaDto[]> {
        return this.apiClient.admin.adminGetMonitorTypeSettings()
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to fetch monitor type settings');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching monitor type settings:', error);
                throw error;
            });
    }

    createMonitor(dto:CreateMonitorConfigRequest): Promise<MonitorConfigurationResponse> {
        return this.apiClient.admin.adminCreateMonitor(dto)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to create monitor');
                }

                return response.data;
            }).catch(error => {
                console.error('Error creating new monitor:', error);
                throw error;
            });
    }

    updateMonitor(id:number, dto:UpdateMonitorConfigRequest): Promise<MonitorConfigurationResponse> {
        return this.apiClient.admin.adminUpdateMonitor(id, dto)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to update monitor');
                }

                return response.data;
            }).catch(error => {
                console.error('Error updating monitor:', error);
                throw error;
            });
    }

    deleteMonitor(id:number): Promise<void> {
        return this.apiClient.admin.adminDeleteMonitor(id)
            .then(response => {
                if(!response.ok) {
                    throw new Error('Failed to delete monitor');
                }
            }).catch(error => {
                console.error('Error deleting monitor:', error);
                throw error;
            });
    }
}