import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { DateTime } from "luxon";
import { interval, Subscription } from "rxjs";

@Component({
    selector: 'app-countdown-timer',
    template: '{{ text }}'
})
export class CountdownTimerComponent implements OnInit, OnDestroy {
    @Input({ required: true }) target!: DateTime;
    private countdownTimer?: Subscription;

    public text = "00:00";

    ngOnInit(): void {
        this.countdownTimer = interval(1000)
            .subscribe(() => {
                this.text = this.target.diffNow(['minutes','seconds']).toFormat('mm:ss');
            });
    }

    ngOnDestroy(): void {
        this.countdownTimer?.unsubscribe();
    }
}