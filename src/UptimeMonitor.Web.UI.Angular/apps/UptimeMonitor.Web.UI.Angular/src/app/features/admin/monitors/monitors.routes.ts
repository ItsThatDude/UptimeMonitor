import { Route } from "@angular/router";
import { MonitorListComponent } from "./components/monitor-list/monitor-list.component";
import { MonitorFormComponent } from "./components/monitor-form/monitor-form.component";

export const monitorRoutes: Route[] = [
    { path: '', component: MonitorListComponent },
    { path: 'edit/:id', component: MonitorFormComponent },
    { path: 'create', component: MonitorFormComponent }
];