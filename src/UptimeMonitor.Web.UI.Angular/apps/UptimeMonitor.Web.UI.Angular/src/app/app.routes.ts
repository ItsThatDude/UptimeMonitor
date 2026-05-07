import { Route } from '@angular/router';
import { StatusPageComponent } from './features/status-page/status-page.component';
import { NotFoundComponent } from './features/not-found/not-found.component';
import { adminRoutes } from './features/admin/admin.routes';
import { authGuard } from './guards/auth.guard';
import { AdminLayoutComponent, BasicLayoutComponent } from './layouts';

export const appRoutes: Route[] = [
    {
        path: '',
        component: BasicLayoutComponent,
        children: [
            { path: '', component: StatusPageComponent },
            { path: 'status-page/:slug', component: StatusPageComponent },
            { path: '404', component: NotFoundComponent }
        ]
    },
    {
        path: 'admin',
        component: AdminLayoutComponent,
        children: [...adminRoutes],
        canActivate: [authGuard]
    }
];
