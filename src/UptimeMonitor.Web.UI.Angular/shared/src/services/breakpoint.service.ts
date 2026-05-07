import { Injectable, computed, inject, Signal } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root'
})
export class BreakpointService {
  private breakpointObserver = inject(BreakpointObserver);

  // Observable of the current breakpoint state, converted to a signal for modern Angular
  private breakpointState: Signal<BreakpointState|undefined> = toSignal(
    this.breakpointObserver.observe([
      Breakpoints.XSmall,
      Breakpoints.Small,
      Breakpoints.Medium,
      Breakpoints.Large,
      Breakpoints.XLarge
    ])
  );

  /**
   * Signal that returns the name of the currently active breakpoint.
   */
  public currentBreakpoint: Signal<string> = computed(() => {
    const state = this.breakpointState();
    if (!state || !state.breakpoints) return 'Unknown';

    const activeBreakpoint = Object.keys(state.breakpoints).find(key => state.breakpoints[key]);
    return activeBreakpoint || 'Unknown';
  });

  /**
   * Helper functions (signals) to check if a specific breakpoint is active
   */
  public isXSmall: Signal<boolean> = computed(() => 
    this.breakpointState()?.breakpoints[Breakpoints.XSmall] || false
  );

  public isSmall: Signal<boolean> = computed(() => 
    this.breakpointState()?.breakpoints[Breakpoints.Small] || false
  );

  public isMedium: Signal<boolean> = computed(() => 
    this.breakpointState()?.breakpoints[Breakpoints.Medium] || false
  );

  public isLarge: Signal<boolean> = computed(() => 
    this.breakpointState()?.breakpoints[Breakpoints.Large] || false
  );

  public isXLarge: Signal<boolean> = computed(() => 
    this.breakpointState()?.breakpoints[Breakpoints.XLarge] || false
  );

  public state: Signal<BreakpointState|undefined> = this.breakpointState;

}