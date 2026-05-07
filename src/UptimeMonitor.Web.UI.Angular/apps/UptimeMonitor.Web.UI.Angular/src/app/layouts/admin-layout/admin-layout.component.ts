import { Component, computed, inject, Signal, signal, TemplateRef, WritableSignal } from "@angular/core";
import { ThemeSelectComponent } from "shared/src/components";
import { AuthService } from "../../services/auth.service";
import { RouterModule } from "@angular/router";
import { NgbDropdownModule, NgbOffcanvas, OffcanvasDismissReasons } from "@ng-bootstrap/ng-bootstrap";
import { NgTemplateOutlet } from "@angular/common";
import { ThemeService } from "shared/src/services/theme.service";

@Component({
  selector: 'app-admin-layout',
  templateUrl: 'admin-layout.component.html',
  styleUrls: ['admin-layout.component.scss'],
  imports: [RouterModule, NgTemplateOutlet, ThemeSelectComponent, NgbDropdownModule]
})
export class AdminLayoutComponent {
  private offcanvasService = inject(NgbOffcanvas);
  private authService = inject(AuthService);

  closeResult: WritableSignal<string> = signal('');

  get isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }
  
  get userName(): string {
    return this.authService.userName();
  }
  
  open(content: TemplateRef<any>) {
		this.offcanvasService.open(content, { ariaLabelledBy: 'sidebarMenuLabel' }).result.then(
			(result) => {
				this.closeResult.set(`Closed with: ${result}`);
			},
			(reason) => {
				this.closeResult.set(`Dismissed ${this.getDismissReason(reason)}`);
			},
		);
	}

	private getDismissReason(reason: any): string {
		switch (reason) {
			case OffcanvasDismissReasons.ESC:
				return 'by pressing ESC';
			case OffcanvasDismissReasons.BACKDROP_CLICK:
				return 'by clicking on the backdrop';
			default:
				return `with: ${reason}`;
		}
	}

	logout() {
		this.authService.logout();
	}
}