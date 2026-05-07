import { Component, OnInit } from "@angular/core";
import { WorkerService } from "../../worker.service";
import { WorkerConfigurationDtoPagedListResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { RouterLink } from "@angular/router";
import { NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem } from "@ng-bootstrap/ng-bootstrap";
import { LoadingService, PaginatorComponent } from "shared/src/components";
import { DateTime } from "luxon";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";
import { ToolbarContentComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/toolbar-content.component";

@Component({
    selector: 'app-worker-list',
    templateUrl: 'worker-list.component.html',
    imports: [PageHeaderComponent, ToolbarContentComponent, RouterLink, NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem, PaginatorComponent]
})
export class WorkerListComponent implements OnInit {
    workers: WorkerConfigurationDtoPagedListResponse = { totalPages: 0, totalRecords: 0, records: [] };
    page: number = 1;
    pageSize: number = 10;

    constructor(
        private readonly workerService: WorkerService,
        private readonly loadingService: LoadingService
    ) {}

    ngOnInit(): void {
        this.reload();
    }

    reload() {
        this.loadingService.loadingOn();
        this.workerService.getWorkers()
            .then(workers => this.workers = workers)
            .finally(() => this.loadingService.loadingOff());
    }

    pageChanged(newPage:number): void {
        this.page = newPage;
        this.loadingService.loadingOn();
        this.workerService.getWorkers(this.page, this.pageSize)
            .then(workers => this.workers = workers)
            .finally(() => this.loadingService.loadingOff());
    }

    deleteWorker(workerId:number): void {
        this.workerService.deleteWorker(workerId)
            .then(() => {
                this.reload();
            })
    }
    
    formatCheckIn(timestamp:string) {
        return DateTime.fromISO(timestamp).toRelative();
    }
}