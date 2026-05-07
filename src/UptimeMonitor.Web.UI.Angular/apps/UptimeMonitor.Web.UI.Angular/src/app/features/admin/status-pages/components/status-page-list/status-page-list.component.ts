import { Component, OnInit } from "@angular/core";
import { RouterLink } from "@angular/router";
import { StatusPageService } from "../../status-page.service";
import { StatusPageSimpleDtoPagedListResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { NgbDropdown, NgbDropdownItem, NgbDropdownMenu, NgbDropdownToggle } from "@ng-bootstrap/ng-bootstrap";
import { LoadingService, PaginatorComponent } from "shared/src/components";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";
import { ToolbarContentComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/toolbar-content.component";

@Component({
    selector: 'app-status-page-list',
    templateUrl: 'status-page-list.component.html',
    imports: [PageHeaderComponent, ToolbarContentComponent, RouterLink, NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem, PaginatorComponent]
})
export class StatusPageListComponent implements OnInit {
    pages: StatusPageSimpleDtoPagedListResponse = { totalPages: 0, totalRecords: 0, records: [] };
    page: number = 1;
    pageSize: number = 10;

    constructor(
        private readonly statusPageService: StatusPageService,
        private readonly loadingService: LoadingService
    ){}

    ngOnInit(): void {
        this.reload();
    }

    reload(): void {
        this.loadingService.loadingOn();
        this.statusPageService.getStatusPages(this.pageSize, this.page)
            .then((pages) => this.pages = pages)
            .finally(() => this.loadingService.loadingOff());
    }

    pageChanged(newPage:number): void {
        this.page = newPage;
        this.reload();
    }

    setDefault(slug:string): void {
        this.statusPageService.setDefault(slug)
            .then(() => this.reload());
    }

    deleteStatusPage(slug:string): void {
        this.statusPageService.deleteStatusPage(slug)
            .then(() => this.reload())
    }
}