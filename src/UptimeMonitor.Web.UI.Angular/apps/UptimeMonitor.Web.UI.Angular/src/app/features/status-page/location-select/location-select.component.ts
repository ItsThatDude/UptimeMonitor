import { NgIf } from "@angular/common";
import { Component, effect, input, output } from "@angular/core";
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { debounceTime, distinctUntilChanged } from "rxjs";

@Component({
    selector: 'app-location-select',
    templateUrl: 'location-select.component.html',
    imports: [NgbModule, ReactiveFormsModule, FormsModule, NgIf]
})
export class LocationSelectComponent {
    readonly selectedLocation = input<string|null>();
    readonly locations = input<string[]>([]);

    readonly selectLocation = output<string|null>();
    readonly searchLocation = output<string|null>();

    searchInput = new FormControl('');

    constructor() {
        effect(() => {
            this.searchInput.valueChanges
                .pipe(debounceTime(500), distinctUntilChanged())
                .subscribe((result) => {
                    this.searchLocation.emit(result);
                })
        })
    }
}