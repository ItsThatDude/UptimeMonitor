import { Route } from "@angular/router";
import { WorkerListComponent } from "./components/worker-list/worker-list.component";
import { WorkerDetailComponent } from "./components/worker-detail/worker-detail.component";
import { WorkerFormComponent } from "./components/worker-form/worker-form.component";

export const workerRoutes: Route[] = [
    { path: '', component: WorkerListComponent },
    { path: 'view/:id', component: WorkerDetailComponent },
    { path: 'edit/:id', component: WorkerFormComponent },
    { path: 'create', component: WorkerFormComponent }
];