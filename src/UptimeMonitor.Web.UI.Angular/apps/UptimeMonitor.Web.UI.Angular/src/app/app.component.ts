import { Component, computed, Renderer2 } from '@angular/core';
import { RouterModule } from '@angular/router';
import { LoadingIndicatorComponent } from 'shared/src/components';
import { ThemeService } from 'shared/src/services/theme.service';
import { AuthService } from './services/auth.service';
import { ConfigService } from './services/config.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [RouterModule, LoadingIndicatorComponent]
})
export class AppComponent {

  readonly isLoading = computed(() => this.authService.isLoading() || this.configService.isLoading());

  constructor(
    private readonly authService: AuthService,
    private readonly configService: ConfigService,
    private readonly renderer: Renderer2,
    themeService: ThemeService
  ) {
    this.authService.fetchUserInfo();
    this.configService.fetchConfig();

    themeService.theme.subscribe(theme => {
      this.renderer.setAttribute(document.querySelector('html'), 'data-bs-theme', theme);
    })
  }

  get isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }
}
