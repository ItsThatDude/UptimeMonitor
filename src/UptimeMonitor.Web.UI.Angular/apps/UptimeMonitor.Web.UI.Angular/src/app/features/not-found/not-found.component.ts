import { Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";

@Component({
    templateUrl: 'not-found.component.html'
})
export class NotFoundComponent implements OnInit {
    constructor(private readonly pageTitle: Title) {}

    ngOnInit(): void {
        this.pageTitle.setTitle('Page not found - UptimeMonitor');
    }

}