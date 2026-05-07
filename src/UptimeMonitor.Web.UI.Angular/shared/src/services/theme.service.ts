import { inject, Injectable, Signal } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { BreakpointService } from "./breakpoint.service";

type ThemeValue = 'light' | 'dark';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    private breakpointService = inject(BreakpointService);
    private readonly _theme = new BehaviorSubject<'light'|'dark'>('light');

    constructor() {
        const storedTheme = localStorage.getItem('theme');

        if(storedTheme !== null && this.isThemeValue(storedTheme)) {
            this.set(storedTheme as ThemeValue, false);
        }
    }

    isThemeValue(theme:string) {
        return theme === 'light' || theme === 'dark';
    }

    set(theme: ThemeValue, save = true): void {
        this._theme.next(theme);

        if(save) {
            localStorage.setItem('theme', theme);
        }
    }

    public theme = this._theme as Observable<string>;

    public breakpoint = this.breakpointService.currentBreakpoint;
}