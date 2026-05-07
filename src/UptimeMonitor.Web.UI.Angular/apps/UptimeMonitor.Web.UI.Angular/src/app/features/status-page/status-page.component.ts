import { Component, computed, Input, OnDestroy, OnInit } from "@angular/core";
import { Api, HeartbeatDto, GetStatusPageResponse } from "@uptime-monitor.web.ui.angular/shared/public-api";
import { interval, Subscription } from "rxjs";
import { DateTime, Duration } from "luxon";
import { Router } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { CountdownTimerComponent } from "./countdown.component";
import { Title } from "@angular/platform-browser";
import { MonitorGroupComponent } from "./monitor-group/monitor-group.component";
import { CommonModule } from "@angular/common";
import { LocationSelectComponent } from "./location-select/location-select.component";
import { WorkerSelectComponent } from "./worker-select/worker-select.component";
import { TimeRangeSelectComponent } from "./time-range-select/time-range-select.component";
import { LoadingService } from "shared/src/components";
import { ConfigService } from "../../services/config.service";

export interface MonitorStats {
    monitorId: number;
    firstTimestamp: DateTime;
    lastTimestamp: DateTime;
    uptimePercent: number;
    responseTime: number;
};

@Component({
    selector: 'app-status-page',
    templateUrl: 'status-page.component.html',
    styleUrl: 'status-page.component.scss',
    imports: [CommonModule, FormsModule, CountdownTimerComponent, MonitorGroupComponent, LocationSelectComponent, WorkerSelectComponent, TimeRangeSelectComponent]
})
export class StatusPageComponent implements OnInit, OnDestroy {
    private refreshInterval: Duration = Duration.fromObject({minutes: 1});
    private refreshTimer?: Subscription;
    public nextUpdate: DateTime = DateTime.now();

    public statusPage?: GetStatusPageResponse;

    public locations: string[] = [];
    public selectedLocation?: string = undefined;
    public workers: string[] = [];
    public selectedWorker?: string = undefined;

    public selectedTimeRange = "1h";

    statsMap: Map<number, MonitorStats> = new Map();
    heartbeatsMap: Map<number, HeartbeatDto[]> = new Map();

    constructor(
        private readonly configService: ConfigService,
        private readonly loadingService: LoadingService,
        private readonly apiClient: Api<string>,
        private readonly router: Router,
        private readonly pageTitle: Title
    ) { }

    public calculateMonitorStats(monitorId: number, uptime: number, heartbeats: HeartbeatDto[]): MonitorStats {
        const first = heartbeats[0];
        const last = heartbeats[heartbeats.length - 1];

        return {
            monitorId: monitorId,
            firstTimestamp: first != undefined ? DateTime.fromISO(first.timestamp) : DateTime.now(),
            lastTimestamp: last != undefined ? DateTime.fromISO(last.timestamp) : DateTime.now(),
            uptimePercent: uptime,
            responseTime: heartbeats.length > 0 ? Math.round(heartbeats.reduce((sum, hb) => sum + (hb.responseTime || 0), 0) / heartbeats.length) : 0
        }
    }

    private readonly defaultPage = computed(() => this.configService.settings()?.defaultStatusPageSlug);

    @Input({ required: true }) slug!: string;

    ngOnInit(): void {
        if(!this.slug || this.slug.length == 0) {
            const slug = this.defaultPage();
            
            if(slug && slug.length > 0) {
                this.slug = slug;
            }
            else {
                this.router.navigate(['/404']);
                return;
            }
        }

        this.update(true);

        this.refreshTimer = interval(this.refreshInterval.toMillis())
            .subscribe(() => { this.update() });
    }

    ngOnDestroy(): void {
        this.refreshTimer?.unsubscribe();
    }

    public searchLocation(location: string|null) {
        this.apiClient.statusPage.getLocations(this.slug, { search: location ?? undefined })
            .then(response => {
                this.locations = response.data;
            })
    }

    public setLocation(location: string|null) {
        this.selectedLocation = location ?? undefined;

        this.update(true);
    }

    public searchWorker(worker: string|null) {
        this.apiClient.statusPage.getWorkers(this.slug, { search: worker ?? undefined, location: this.selectedLocation ?? undefined })
            .then(response => {
                this.workers = response.data;
            })
    }

    public setWorker(worker: string|null) {
        this.selectedWorker = worker ?? undefined;

        this.update(true);
    }

    public setTimeRange(timeRange: string|null) {
        this.selectedTimeRange = timeRange ?? "1H";

        this.update(true);
    }

    public update(showLoading = false) {
        if(showLoading) {
            this.loadingService.loadingOn();
        }

        const heartbeatParams = {
            location: this.selectedLocation ?? undefined,
            worker: this.selectedWorker ?? undefined,
            timeRange: this.selectedTimeRange ?? undefined
        };

        Promise.all([
            this.apiClient.statusPage.getStatusPage(this.slug),
            this.apiClient.statusPage.getLocations(this.slug),
            this.apiClient.statusPage.getWorkers(this.slug, { location: this.selectedLocation ?? undefined }),
            this.apiClient.statusPage.getHeartbeats(this.slug, heartbeatParams)
        ])
        .then(([pageResponse, locationsResponse, workersResponse, heartbeatsResponse]) => {
            this.statusPage = pageResponse.data;
            this.locations = locationsResponse.data;
            this.workers = workersResponse.data;

            this.pageTitle.setTitle(this.statusPage.name + " - UptimeMonitor");
            
            const monitorData = heartbeatsResponse.data;
            monitorData.forEach(monitor => {
                monitor.heartbeats = monitor.heartbeats.slice(1);

                this.statsMap.set(monitor.monitorId, this.calculateMonitorStats(monitor.monitorId, monitor.uptime, monitor.heartbeats));
                this.heartbeatsMap.set(monitor.monitorId, monitor.heartbeats);
            });
        })
        .catch(() => {
            this.router.navigate(['/404']);
        })
        .finally(() => {
            if(showLoading) {
                this.loadingService.loadingOff()
            }
        });
        
        this.nextUpdate = DateTime.now().plus(this.refreshInterval);
    }

    trackById(index: number, item: { id: number }) {
        return item.id;
    }

    public getStats(monitorId: number): MonitorStats | undefined {
        return this.statsMap.get(monitorId);
    }

    public getHeartbeats(monitorId: number): HeartbeatDto[] {
        return this.heartbeatsMap.get(monitorId) || [];
    }

    public nextUpdateString() {
        return this.nextUpdate.toLocaleString(DateTime.TIME_SIMPLE);
    }
}