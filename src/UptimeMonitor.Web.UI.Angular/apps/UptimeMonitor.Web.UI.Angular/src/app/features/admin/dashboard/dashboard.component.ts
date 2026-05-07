import { Component, OnInit } from "@angular/core";
import { MonitorService } from "../monitors/monitors.service";
import { GetMonitorHeartbeatsResponse, GetMonitorStatisticsResponse, GetWorkerStatisticsResponse, MonitorEventDtoPagedListResponse, MonitorSimpleDtoPagedListResponse, WorkerConfigurationDto, WorkerConfigurationDtoPagedListResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { BaseChartDirective } from "ng2-charts";
import { ChartConfiguration } from "chart.js";
import { enUS } from "date-fns/locale";
import { parseISO } from "date-fns";
import { WorkerService } from "../workers/worker.service";
import { DateTime } from "luxon";
import { findOutages } from "@uptime-monitor.web.ui.angular/shared";
import { AnnotationOptions, AnnotationTypeRegistry } from "chartjs-plugin-annotation";
import { _DeepPartialObject } from "chart.js/dist/types/utils";
import { LoadingService, PaginatorComponent } from "shared/src/components";

@Component({
    selector: 'app-dashboard',
    templateUrl: 'dashboard.component.html',
    styleUrl: 'dashboard.component.scss',
    imports: [BaseChartDirective, PaginatorComponent]
})
export class DashboardComponent implements OnInit {
    monitors: MonitorSimpleDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};
    heartbeats: {[id: number]: GetMonitorHeartbeatsResponse} = {};
    monitorEvents: MonitorEventDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};
    workers: WorkerConfigurationDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};

    monitorStats?: GetMonitorStatisticsResponse;
    workerStats?: GetWorkerStatisticsResponse;

    monitorsPage = 1;
    monitorsPageSize = 10;
    monitorEventsPage = 1;
    monitorEventsPageSize = 10;
    workersPage = 1;
    workersPageSize = 10;

    constructor(private readonly monitorService: MonitorService,
        private readonly workerService: WorkerService,
        private readonly loadingService: LoadingService
    ) {

    }

    ngOnInit(): void {
        this.reload();
    }

    reload() {
        this.reloadStatistics();
        this.reloadMonitors();
        this.reloadEvents();
        this.reloadWorkers();
    }

    reloadStatistics() {
        this.loadingService.loadingOn();
        this.monitorService.getStatistics()
            .then(stats => this.monitorStats = stats)
            .finally(() => this.loadingService.loadingOff());
        
        this.loadingService.loadingOn();
        this.workerService.getStatistics()
            .then(stats => this.workerStats = stats)
            .finally(() => this.loadingService.loadingOff())
    }

    reloadMonitors() {
        this.loadingService.loadingOn();
        this.monitorService.getMonitors()
            .then((monitors) => {
                this.monitors = monitors;

                this.monitorService.getChartDataForMonitors(this.monitors.records.map(r => r.id))
                    .then((heartbeats) => {
                        this.heartbeats = [];
                        this.monitors.records.forEach(monitor => {
                            const hb = heartbeats.find(hb => hb.monitorId === monitor.id);

                            if(hb) {
                                this.heartbeats[monitor.id] = hb;
                            }
                        })
                    });
            }).finally(() => this.loadingService.loadingOff());
    }

    reloadEvents() {
        this.loadingService.loadingOn();
        this.monitorService.getEvents(this.monitorEventsPage, this.monitorEventsPageSize)
            .then(events => this.monitorEvents = events)
            .finally(() => this.loadingService.loadingOff());
    }

    reloadWorkers() {
        this.loadingService.loadingOn();
        this.workerService.getWorkers(this.workersPage, this.workersPageSize)
            .then(workers => this.workers = workers)
            .finally(() => this.loadingService.loadingOff());
    }

    monitorsPageChanged(page:number) {
        this.monitorsPage = page;
        this.reloadMonitors();
    }

    monitorEventsPageChanged(page:number) {
        this.monitorEventsPage = page;
        this.reloadEvents();
    }

    workersPageChanged(page:number) {
        this.workersPage = page;
        this.reloadWorkers();
    }

    getChartOptions(monitorId:number) {
        const options:ChartConfiguration<'bar'>['options'] = {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'timeseries',
                    display: false,
                    time: {
                        unit: 'minute'
                    },
                    adapters: {
                        date: {
                            locale: enUS
                        }
                    },
                    ticks: {
                        source: 'auto',
                        autoSkip: true,
                        maxTicksLimit: 60,
                        maxRotation: 90,
                        minRotation: 60
                    }
                },
                y: {
                    display: false,
                    min: 0
                },
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }

        if(!options) {
            return {};
        }

        if(!this.heartbeats[monitorId]) {
            return options;
        }

        const outages = findOutages(this.heartbeats[monitorId].heartbeats);
        const annotations:_DeepPartialObject<Record<string, AnnotationOptions<keyof AnnotationTypeRegistry>>> = {};

        if (outages.length > 0) {
            outages.forEach(outage => {
                    const start = parseISO(outage.start).valueOf();
                    const end = parseISO(outage.end).valueOf();
                    
                    annotations['outage_' + start + '_' + end] = {
                        type: 'box',
                        xMin: start,
                        xMax: end,
                        backgroundColor: 'rgba(255, 99, 132, 0.25)'
                    };
            })

            if(options.plugins) {
                options.plugins.annotation = {
                    annotations: annotations
                }
            }
        }

        return options;
    }

    getChartData(monitorId:number) {
        if(!this.heartbeats[monitorId]) {
            return undefined;
        }

        return {
            labels: this.heartbeats[monitorId].heartbeats.map(hb => parseISO(hb.timestamp)),
            datasets: [{
                label: 'Response Time (ms)',
                data: this.heartbeats[monitorId].heartbeats.map(hb => hb.responseTime)
            }]
        };
    }

    formatCheckIn(timestamp:string) {
        return DateTime.fromISO(timestamp).toRelative();
    }

    formatDateTime(timestamp:string) {
        return DateTime.fromISO(timestamp)
            .toLocaleString({ dateStyle: 'short', timeStyle: 'short'});
    }
}