import { Component, ContentChild, Input, OnInit, TemplateRef } from "@angular/core";
import { RouteConfigLoadEnd, RouteConfigLoadStart, Router } from "@angular/router";
import { Observable, tap } from "rxjs";
import { LoadingService } from "../../services/loading-service";
import { AsyncPipe, NgTemplateOutlet } from "@angular/common";

@Component({
    selector: 'lib-loading-indicator',
    templateUrl: 'loading-indicator.component.html',
    styleUrl: 'loading-indicator.component.scss',
    imports: [AsyncPipe, NgTemplateOutlet]
})
export class LoadingIndicatorComponent implements OnInit {
    loading$: Observable<boolean>;

    @Input() detectRouteTransitions = false;

    /* eslint-disable @typescript-eslint/no-explicit-any */
    @ContentChild("loading")
    customLoadingIndicator: TemplateRef<any> | null = null;
    /* eslint-enable @typescript-eslint/no-explicit-any */

    constructor(
        private loadingService: LoadingService,
        private router: Router
    ) {
        this.loading$ = this.loadingService.loading$;
    }

    ngOnInit(): void {
        if (this.detectRouteTransitions) {
            this.router.events.pipe(tap((event) => {
                if (event instanceof RouteConfigLoadStart) {
                    this.loadingService.loadingOn();
                } else if (event instanceof RouteConfigLoadEnd) {
                    this.loadingService.loadingOff();
                }
            })).subscribe();
        }
    }
}