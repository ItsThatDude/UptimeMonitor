import { Component, Input } from "@angular/core";
import { MonitorComponent } from "../monitor/monitor.component";
import { HeartbeatDto, StatusPageMonitorGroupDto } from "@uptime-monitor.web.ui.angular/shared/public-api";
import { MonitorStats } from "../status-page.component";
import { CommonModule } from "@angular/common";

@Component({
    selector: 'app-monitor-group',
    templateUrl: 'monitor-group.component.html',
    imports: [CommonModule, MonitorComponent]
})
export class MonitorGroupComponent {
    @Input() group!: StatusPageMonitorGroupDto;
    @Input() getStats!: (monitorId: number) => MonitorStats | undefined;
    @Input() getHeartbeats!: (monitorId: number) => HeartbeatDto[];

    trackById(index: number, item: { id: number }) {
        return item.id;
    }
}