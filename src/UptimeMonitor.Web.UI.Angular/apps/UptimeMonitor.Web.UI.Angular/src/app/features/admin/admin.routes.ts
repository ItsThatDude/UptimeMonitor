import { Route } from "@angular/router";
import { workerRoutes } from "./workers/worker.routes";
import { statusPageRoutes } from "./status-pages/status-page.routes";
import { monitorRoutes } from "./monitors/monitors.routes";
import { DashboardComponent } from "./dashboard/dashboard.component";

export const adminRoutes: Route[] = [
    {
        path: '',
        children: [
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            },
            {
                path: 'dashboard',
                component: DashboardComponent,
                pathMatch: 'full'
            },
            {
                path: 'monitors',
                children: [...monitorRoutes]
            },
            {
                path: 'status-pages',
                children: [...statusPageRoutes]
            },
            {
                path: 'workers',
                children: [...workerRoutes]
            }
        ]
    }
];