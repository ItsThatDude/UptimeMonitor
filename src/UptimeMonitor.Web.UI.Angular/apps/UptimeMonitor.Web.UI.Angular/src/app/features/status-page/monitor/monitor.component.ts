import { Component, Input } from "@angular/core";
import { HeartbeatDto, StatusPageMonitorDto } from "@uptime-monitor.web.ui.angular/shared/public-api";
import { MonitorStats } from "../status-page.component";
import { CommonModule } from "@angular/common";
import { HeartbeatListComponent } from "../heartbeat-list/heartbeat-list.component";

@Component({
    selector: 'app-monitor',
    templateUrl: 'monitor.component.html',
    imports: [CommonModule, HeartbeatListComponent]
})
export class MonitorComponent {
    @Input() monitor!: StatusPageMonitorDto;
    @Input() stats?: MonitorStats;
    @Input() heartbeats: HeartbeatDto[] = [];
}