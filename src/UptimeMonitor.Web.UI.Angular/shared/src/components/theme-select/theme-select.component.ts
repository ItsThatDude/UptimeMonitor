import { Component, inject, OnInit } from "@angular/core";
import { ThemeService } from "../../services/theme.service";

@Component({
    selector: 'lib-theme-select',
    template: `
        <div class="d-flex align-items-center">
            <i class="bi-sun me-2"></i>
            <button type="button" class="btn btn-xs btn-secondary btn-toggle mx-1"
                data-toggle="button" [class.active]="state" autocomplete="off" (click)="toggle()">
                <div class="handle"></div>
            </button>
            <i class="bi-moon ms-2"></i>
        </div>
    `,
    styleUrl: 'theme-select.component.scss'
})
export class ThemeSelectComponent implements OnInit {
    private readonly themeService = inject(ThemeService);

    public state = false;

    ngOnInit(): void {
        this.themeService.theme.subscribe(theme => {
            if(theme == 'dark')
                this.state = true;
            else
                this.state = false;
        });
    }

    toggle() {
        this.state = !this.state;
        this.themeService.set(this.state ? 'dark' : 'light');
    }
}