import { Component, computed, inject, OnInit, ViewChild } from "@angular/core";
import { RouterLink } from "@angular/router";
import { MonitorService } from "../../monitors.service";
import { MonitorConfigurationResponse, MonitorEventDtoPagedListResponse, MonitorSimpleDto, MonitorSimpleDtoPagedListResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem, NgbAccordionModule } from "@ng-bootstrap/ng-bootstrap";
import { LoadingService, PaginatorComponent } from "shared/src/components";
import { BaseChartDirective } from "ng2-charts";
import { ChartConfiguration, ChartData } from "chart.js";
import { enUS } from 'date-fns/locale';
import { parseISO } from "date-fns";
import { findOutages } from "@uptime-monitor.web.ui.angular/shared";
import { _DeepPartialObject } from "chart.js/dist/types/utils";
import { AnnotationOptions, AnnotationTypeRegistry } from "chartjs-plugin-annotation";
import { DateTime } from "luxon";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";
import { ToolbarContentComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/toolbar-content.component";
import { BreakpointService } from "shared/src/services/breakpoint.service";
import { NgTemplateOutlet } from "@angular/common";

@Component({
    selector: 'app-monitor-list',
    templateUrl: 'monitor-list.component.html',
    imports: [NgTemplateOutlet, PageHeaderComponent, ToolbarContentComponent, RouterLink, NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem, NgbAccordionModule, BaseChartDirective, PaginatorComponent]
})
export class MonitorListComponent implements OnInit {
    breakpointService = inject(BreakpointService);
    isHandset = computed(() => this.breakpointService.isXSmall() || this.breakpointService.isSmall());

    monitors: MonitorSimpleDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};
    monitorsPageSize = 10;
    monitorsPage = 1;

    selectedMonitor?: MonitorConfigurationResponse;
    monitorEvents: MonitorEventDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};
    monitorEventsPageSize = 10;
    monitorEventsPage = 1;

    @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
    public barChartOptions: ChartConfiguration<'bar'>['options'] = {
        animation: false,
        scales: {
            x: {
                type: 'timeseries',
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
                min: 0,
            },
        },
        plugins: {
            legend: {
                display: true,
            },
            annotation: {
                annotations: {}
            }
        },
    };

    public barChartData: ChartData<'bar'> = {
        labels: [],
        datasets: []
    };

    constructor(private readonly monitorService: MonitorService,
        private readonly loadingService: LoadingService
    ) { }

    ngOnInit(): void {
        this.reloadMonitors();
    }

    reloadMonitors(): void {
        this.loadingService.loadingOn();

        this.monitorService.getMonitors(this.monitorsPageSize, this.monitorsPage)
            .then(monitors => this.monitors = monitors)
            .finally(() => this.loadingService.loadingOff());
    }

    selectMonitor(monitor?: MonitorSimpleDto) {
        if(!monitor || (monitor && this.selectedMonitor && monitor.id == this.selectedMonitor.id)) {
            this.selectedMonitor = undefined;
            return;
        }

        this.monitorEventsPage = 1;

        this.monitorService.getMonitor(monitor.id).then(monitor => {
            this.selectedMonitor = monitor;

            if (this.selectedMonitor) {
                this.reloadMonitorEvents();
                this.reloadMonitorChart();
            }
        });
    }

    reloadMonitorEvents() {
        if(!this.selectedMonitor)
            return;

        this.monitorService.getEventsForMonitor(this.selectedMonitor.id, this.monitorEventsPageSize, this.monitorEventsPage)
                    .then((data) => this.monitorEvents = data);
    }

    reloadMonitorChart() {
        if(!this.selectedMonitor)
            return;

        this.monitorService.getChartDataForMonitor(this.selectedMonitor.id)
            .then((data) => {
                if (this.barChartOptions?.plugins?.annotation) {
                    const outages = findOutages(data.heartbeats);

                    this.barChartOptions.plugins.annotation.annotations = {}

                    if (outages.length > 0) {
                        outages.forEach(outage => {
                            if (this.barChartOptions?.plugins?.annotation) {
                                const start = parseISO(outage.start).valueOf();
                                const end = parseISO(outage.end).valueOf();
                                
                                (<_DeepPartialObject<Record<string, AnnotationOptions<keyof AnnotationTypeRegistry>>>>this.barChartOptions.plugins.annotation.annotations)['outage_' + start + '_' + end] = {
                                    type: 'box',
                                    xMin: start,
                                    xMax: end,
                                    backgroundColor: 'rgba(255, 99, 132, 0.25)'
                                };
                            }
                        })
                    }
                }
                this.barChartData = {
                    labels: data.heartbeats.map(hb => parseISO(hb.timestamp)),
                    datasets: [
                        {
                            label: 'Response Time (ms)',
                            data: data.heartbeats.map(hb => hb.responseTime)
                        }
                    ]
                };

                this.chart?.update();
            });
    }

    deleteMonitor(id: number): void {
        this.monitorService.deleteMonitor(id)
            .then(() => {
                this.reloadMonitors();
            })
    }

    monitorsPageChanged(page:number) {
        this.monitorsPage = page;

        this.reloadMonitors();
    }

    monitorEventsPageChanged(page:number) {
        this.monitorEventsPage = page;

        this.reloadMonitorEvents();
    }

    toRelativeDate(date:string): string {
        return DateTime.fromISO(date).toRelativeCalendar() ?? "";
    }

    formatDate(date:string): string {
        return DateTime.fromISO(date).toLocaleString();
    }

    formatDateTime(timestamp:string) {
        return DateTime.fromISO(timestamp)
            .toLocaleString({ dateStyle: 'short', timeStyle: 'short'});
    }
}