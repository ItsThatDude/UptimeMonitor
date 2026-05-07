import { Component, OnInit } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { WorkerService } from "../../worker.service";
import { ActivatedRoute, Router } from "@angular/router";
import { LoadingService } from "shared/src/components";
import { WorkerConfigurationDto } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";

interface FormValue {
    name: string,
    location: string,
    secret: string,
    configUpdateInterval: number
}

@Component({
    templateUrl: 'worker-form.component.html',
    imports: [PageHeaderComponent, ReactiveFormsModule]
})
export class WorkerFormComponent implements OnInit {
    id: number | null = null;
    isEditMode = false;

    secretCopied = false;

    form = new FormGroup({
        name: new FormControl('', { nonNullable: true, validators: [Validators.required]}),
        location: new FormControl('', { nonNullable: true, validators: [Validators.required]}),
        secret: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)]}),
        configUpdateInterval: new FormControl(60, { nonNullable: true, validators: [Validators.required, Validators.min(60)]})
    })

    get formValue(): FormValue {
        return this.form.value as FormValue;
    }

    constructor(
        private readonly workerService: WorkerService,
        private readonly loadingService: LoadingService,
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) {}

    ngOnInit(): void {
        this.id = Number(this.route.snapshot.paramMap.get('id'));
        this.isEditMode = this.route.snapshot.paramMap.has('id');

        if(this.id) {
            this.loadingService.loadingOn();

            this.workerService.getWorker(this.id)
                .then((worker) => this.populateForm(worker))
                .finally(() => this.loadingService.loadingOff());
        }
    }

    private populateForm(worker:WorkerConfigurationDto): void {
        this.form.patchValue({
            name: worker.name,
            location: worker.location,
            configUpdateInterval: worker.configUpdateInterval
        });

        this.form.controls['secret'].clearValidators();
        this.form.controls['secret'].updateValueAndValidity();
    }

    private generateRandomString(length: number): string {
        const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()'; // You can adjust this charset as needed
        let result = '';

        // Generate the random string using the crypto API
        const randomValues = new Uint8Array(length);
        window.crypto.getRandomValues(randomValues);

        // Map each random byte to a character from the charset
        for (let i = 0; i < length; i++) {
            result += charset[randomValues[i] % charset.length];
        }

        return result;
    }

    get allowSecretCopyToClipboard(): boolean {
        return this.form.controls['secret'].invalid || this.formValue.secret.length === 0
    }

    generateSecret(): void {
        this.form.patchValue({ secret: this.generateRandomString(32) });
    }

    copySecret(): void {
        if(this.formValue.secret.length > 0) {
            navigator.clipboard.writeText(this.formValue.secret);
            this.secretCopied = true;

            setTimeout(() => {
                this.secretCopied = false;
            }, 3000);
        }
    }

    onSubmit() {
        if (this.form.invalid || !this.form.value.name || !this.form.value.location) {
            return;
        }

        if (this.isEditMode && !this.id) {
            console.error('ID is missing in edit mode');
            return;
        }
        
        const payload = {
            name: this.formValue.name,
            location: this.formValue.location,
            configUpdateInterval: this.formValue.configUpdateInterval,
            secret: this.formValue.secret
        };
        
        const operation = this.isEditMode && this.id
            ? this.workerService.updateWorker(this.id, payload)
            : this.workerService.createWorker(payload);

        operation.then(() => {
            this.router.navigate(['../../'], { relativeTo: this.route });
        });
    }
}