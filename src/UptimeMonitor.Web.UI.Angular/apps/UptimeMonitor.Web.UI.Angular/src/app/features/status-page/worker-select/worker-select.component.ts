import { NgIf } from "@angular/common";
import { Component, effect, input, output } from "@angular/core";
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { debounceTime, distinctUntilChanged } from "rxjs";

@Component({
    selector: 'app-worker-select',
    templateUrl: 'worker-select.component.html',
    imports: [NgbModule, ReactiveFormsModule, FormsModule, NgIf]
})
export class WorkerSelectComponent {
    readonly selectedWorker = input<string|null>();
    readonly workers = input<string[]>([]);

    readonly selectWorker = output<string|null>();
    readonly searchWorker = output<string|null>();

    searchInput = new FormControl('');

    constructor() {
        effect(() => {
            this.searchInput.valueChanges
                .pipe(debounceTime(500), distinctUntilChanged())
                .subscribe((result) => {
                    this.searchWorker.emit(result);
                })
        })
    }
}