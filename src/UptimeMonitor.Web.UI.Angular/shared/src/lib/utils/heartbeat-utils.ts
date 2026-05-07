import { HeartbeatDto } from "@uptime-monitor.web.ui.angular/shared/admin-api";

interface Outage {
    start: string; // ISO timestamp
    end: string;   // ISO timestamp
    durationMs: number;
}

export function findOutages(heartbeats: HeartbeatDto[]): Outage[] {
    if (!heartbeats.length) return [];

    // Ensure heartbeats are sorted by timestamp
    const sorted = [...heartbeats].sort(
        (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    );

    const outages: Outage[] = [];
    let outageStart: string | null = null;

    for (const hb of sorted) {
        const isUp = hb.up ?? true; // assume 'true' if up is undefined/null

        if (!isUp) {
            if (!outageStart) {
                outageStart = hb.timestamp; // start of a new outage
            }
        } else {
            if (outageStart) {
                // end of an outage
                const outageEnd = hb.timestamp;
                outages.push({
                    start: outageStart,
                    end: outageEnd,
                    durationMs: new Date(outageEnd).getTime() - new Date(outageStart).getTime(),
                });
                outageStart = null; // reset
            }
        }
    }

    // If the last heartbeat was down, the outage is still ongoing
    if (outageStart) {
        const last = sorted[sorted.length - 1].timestamp;
        outages.push({
            start: outageStart,
            end: last,
            durationMs: new Date(last).getTime() - new Date(outageStart).getTime(),
        });
    }

    return outages;
}