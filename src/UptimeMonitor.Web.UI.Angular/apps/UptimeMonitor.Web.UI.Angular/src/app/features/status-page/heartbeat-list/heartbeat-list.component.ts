import { CommonModule } from "@angular/common";
import { Component, Input } from "@angular/core";
import { HeartbeatDto } from "@uptime-monitor.web.ui.angular/shared/public-api";
import { DateTime } from "luxon";

@Component({
    selector: 'app-heartbeat-list',
    templateUrl: 'heartbeat-list.component.html',
    styleUrl: 'heartbeat-list.component.scss',
    imports: [CommonModule]
})
export class HeartbeatListComponent {
    @Input() heartbeats: HeartbeatDto[] = [];

    trackByTimestamp(index: number, heartbeat: HeartbeatDto) {
        return heartbeat.timestamp;
    }

    formatTitle(heartbeat: HeartbeatDto): string {
        const dateTimeString = DateTime.fromISO(heartbeat.timestamp).toLocaleString(DateTime.DATETIME_FULL_WITH_SECONDS);
        return `${dateTimeString} (${heartbeat.upHeartbeats}/${heartbeat.totalHeartbeats})`;
    }
}