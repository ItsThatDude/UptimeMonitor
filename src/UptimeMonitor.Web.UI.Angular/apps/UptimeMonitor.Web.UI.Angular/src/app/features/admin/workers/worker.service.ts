import { inject, Injectable } from "@angular/core";
import { Api, CreateWorkerRequest, UpdateWorkerRequest, WorkerConfigurationDto, GetWorkerStatisticsResponse, WorkerConfigurationDtoPagedListResponse } from '@uptime-monitor.web.ui.angular/shared/admin-api'

@Injectable({ providedIn: 'root' })
export class WorkerService {
    private readonly apiClient: Api<string> = inject(Api<string>);

    getStatistics(): Promise<GetWorkerStatisticsResponse> {
        return this.apiClient.admin.getWorkerStatistics()
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch worker stats');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching worker stats:', error);
                throw error;
            });
    }

    getWorkers(page?: number, pageSize?: number): Promise<WorkerConfigurationDtoPagedListResponse> {
        return this.apiClient.admin.getWorkerList({ pageSize: pageSize, page: page })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch workers');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching workers:', error);
                throw error;
            });
    }

    getWorker(id: number): Promise<WorkerConfigurationDto> {
        return this.apiClient.admin.getWorkerById(id)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch worker');
                }

                return response.data;
            }).catch(error => {
                console.error('Error fetching worker:', error);
                throw error;
            });
    }

    createWorker(data: CreateWorkerRequest): Promise<WorkerConfigurationDto> {
        return this.apiClient.admin.createWorker(data)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to create worker');
                }

                return response.data;
            }).catch(error => {
                console.error('Error creating worker:', error);
                throw error;
            });
    }

    updateWorker(id: number, data: UpdateWorkerRequest): Promise<WorkerConfigurationDto> {
        return this.apiClient.admin.updateWorker(id, data)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to update worker');
                }

                return response.data;
            }).catch(error => {
                console.error('Error updating worker:', error);
                throw error;
            });
    }

    deleteWorker(workerId: number): Promise<void> {
        return this.apiClient.admin.deleteWorker(workerId)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to delete worker');
                }
            }).catch(error => {
                console.error('Error deleting worker:', error);
                throw error;
            });
    }
}