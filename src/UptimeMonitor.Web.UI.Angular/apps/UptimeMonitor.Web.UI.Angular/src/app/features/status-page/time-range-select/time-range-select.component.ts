import { NgIf } from "@angular/common";
import { Component, input, output } from "@angular/core";
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

@Component({
    selector: 'app-time-range-select',
    templateUrl: 'time-range-select.component.html',
    imports: [NgbModule, ReactiveFormsModule, FormsModule, NgIf]
})
export class TimeRangeSelectComponent {
    readonly selectedTimeRange = input<string>("1h");
    readonly selectTimeRange = output<string|null>();

    validTimeRanges = ["1h", "24h"];
}