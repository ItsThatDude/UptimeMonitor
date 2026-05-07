import { Route } from "@angular/router";
import { StatusPageListComponent } from "./components/status-page-list/status-page-list.component";
import { StatusPageFormComponent } from "./components/status-page-form/status-page-form.component";

export const statusPageRoutes: Route[] = [
    { path: '', component: StatusPageListComponent },
    //{ path: 'view/:id', component: WorkerDetailComponent },
    { path: 'edit/:slug', component: StatusPageFormComponent },
    { path: 'create', component: StatusPageFormComponent }
];