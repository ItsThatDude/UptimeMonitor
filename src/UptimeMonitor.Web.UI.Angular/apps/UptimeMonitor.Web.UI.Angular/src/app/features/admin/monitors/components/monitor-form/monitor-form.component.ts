import { Component, OnInit } from "@angular/core";
import { MonitorService } from "../../monitors.service";
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { MonitorConfigurationResponse, MonitorTypeDto, MonitorSettingsSchemaDto } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { ActivatedRoute, Router } from "@angular/router";
import { NgFor, NgIf } from "@angular/common";
import { LoadingService } from "shared/src/components";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";

interface MonitorFormValue {
    name: string;
    type: string;
    target: string;
    enabled: boolean;
    interval: number;
    timeout: number;
    warningThreshold: number;
    settings: [];
}

@Component({
    selector: 'app-monitor-form',
    templateUrl: 'monitor-form.component.html',
    imports: [PageHeaderComponent, ReactiveFormsModule, NgIf, NgFor]
})
export class MonitorFormComponent implements OnInit {
    id: number | null = null;
    isEditMode = false;

    types: MonitorTypeDto[] = [];
    typeSettings: MonitorSettingsSchemaDto[] = [];
    selectedTypeSettings: MonitorSettingsSchemaDto | null = null;

    form = new FormGroup({
        name: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        type: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        target: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        enabled: new FormControl(true, { nonNullable: true, validators: [Validators.required] }),
        interval: new FormControl(60, { nonNullable: true, validators: [Validators.required] }),
        timeout: new FormControl(10, { nonNullable: true, validators: [Validators.required] }),
        warningThreshold: new FormControl(1000, { nonNullable: true, validators: [Validators.required] }),
        settings: new FormArray([])
    })

    get formSettings(): FormArray {
        return this.form.get('settings') as FormArray;
    }

    get formValue(): MonitorFormValue {
        return this.form.value as MonitorFormValue;
    }

    constructor(
        private readonly monitorService: MonitorService,
        private readonly loadingService: LoadingService,
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) {}

    ngOnInit(): void {
        this.id = Number(this.route.snapshot.paramMap.get('id'));
        this.isEditMode = this.route.snapshot.paramMap.has('id');

        this.loadingService.loadingOn();

        Promise.all([
            this.monitorService.getMonitorTypes(),
            this.monitorService.getMonitorTypeSettings()
        ]).then(([types, typeSettings]) => {
            this.types = types;
            this.typeSettings = typeSettings;

            if (this.id) {
                this.loadingService.loadingOn();
                this.monitorService.getMonitor(this.id)
                    .then(monitor => this.populateForm(monitor))
                    .finally(() => this.loadingService.loadingOff());
            }
        }).finally(() => this.loadingService.loadingOff());

        this.form.controls['type'].valueChanges.subscribe(change => {
            this.updateTypeSettings(change);
        });
    }

    getTypeDisplayName(type: string): string {
        return this.types.find(t => t.key == type)?.displayName ?? type;
    }

    populateForm(monitor: MonitorConfigurationResponse): void {
        this.form.patchValue({
            name: monitor.name,
            type: monitor.type,
            target: monitor.target,
            enabled: monitor.enabled,
            interval: monitor.interval,
            timeout: monitor.timeout,
            warningThreshold: monitor.warningThreshold,
        });

        this.form.controls['type'].disable();

        // Update form settings based on the monitor's type
        this.updateTypeSettings(monitor.type, monitor.settings);
    }

    updateTypeSettings(type: string, settingsJson?:string): void {
        const selectedSchema = this.typeSettings.find(schema => schema.monitorType === type);
        this.selectedTypeSettings = selectedSchema ?? null;

        // Clear existing form controls in the typeSettings FormArray
        this.formSettings.clear();

        if (selectedSchema) {
            // Parse the JSON string into an object
            const parsedSettings = JSON.parse(settingsJson ?? "{}");

            // Add each property to the FormArray
            selectedSchema.properties.forEach((property) => {
                let settingValue = parsedSettings[property.key];

                if(property.allowMultiple && settingValue) {
                    settingValue = (settingValue as string[]).join(',')
                }

                const formControl = new FormControl(settingValue || property.defaultValue, property.required ? Validators.required : []);
                this.formSettings.push(formControl);
            });

            // Now set the values from the parsed JSON object to the corresponding controls
            this.formSettings.controls.forEach((control, index) => {
                const property = selectedSchema.properties[index];
                if (parsedSettings[property.key]) {
                    control.setValue(parsedSettings[property.key]);
                } else {
                    control.setValue(property.defaultValue); // set default value if not present in parsed JSON
                }
            });
        }
    }

    getSettingValuesAsJson(): string {
        const settingsObject: { [key: string]: string|string[]|number|number[]|boolean|null } = {};
    
        // Loop through each control in the FormArray
        this.formSettings.controls.forEach((control, index) => {
            if(!this.selectedTypeSettings) {
                return;
            }

            const property = this.selectedTypeSettings.properties[index];
            const key = property.key; 
            let value = control.value;

            switch(property.dataType) {
                case 'Int':
                    if(property.allowMultiple) {
                        if(typeof(value) !== 'number' && typeof(value) !== 'object') {
                            value = (value as string).split(',').map(s => Number(s))
                        }
                    }
                    else {
                        value = Number(value);
                    }
                    break;
                case 'Boolean':
                    value = Boolean(value);
                    break;
                default:
                    break;
            }
    
            // Assign the value to the key in the settings object
            settingsObject[key] = value;
        });
    
        // Convert the settings object to a JSON string
        return JSON.stringify(settingsObject);
    }

    onSubmit() {
        if (this.form.invalid || !this.form.value.name || !this.form.value.target) {
            return;
        }

        if (this.isEditMode && !this.id) {
            console.error('ID is missing in edit mode');
            return;
        }

        const settingsAsJson = this.getSettingValuesAsJson();

        const payload = {
            name: this.formValue.name,
            type: this.formValue.type,
            target: this.formValue.target,
            enabled: this.formValue.enabled,
            interval: this.formValue.interval,
            timeout: this.formValue.timeout,
            warningThreshold: this.formValue.warningThreshold,
            settings: settingsAsJson
        };
        
        const operation = this.isEditMode && this.id
            ? this.monitorService.updateMonitor(this.id, payload)
            : this.monitorService.createMonitor(payload);

        operation.then(() => {
            this.router.navigate(['../../'], { relativeTo: this.route });
        });
    }
}