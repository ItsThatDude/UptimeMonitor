import { Component } from "@angular/core";
import { AuthService } from "../../services/auth.service";
import { ThemeSelectComponent } from "shared/src/components";
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-basic-layout',
  templateUrl: 'basic-layout.component.html',
  styleUrls: ['basic-layout.component.scss'],
  imports: [RouterModule, ThemeSelectComponent]
})
export class BasicLayoutComponent {
  constructor(
    private readonly authService: AuthService
  ) {}

  get isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }
}