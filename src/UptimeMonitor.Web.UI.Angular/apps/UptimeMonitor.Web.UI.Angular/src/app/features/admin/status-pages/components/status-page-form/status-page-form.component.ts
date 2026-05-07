import { Component, OnInit } from "@angular/core";
import { StatusPageService } from "../../status-page.service";
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { NgFor } from "@angular/common";
import { MonitorSimpleDto, MonitorSimpleDtoPagedListResponse, StatusPageConfigResponse } from "@uptime-monitor.web.ui.angular/shared/admin-api";
import { MonitorService } from "../../../monitors/monitors.service";
import { LoadingService } from "shared/src/components";
import { DndDraggableDirective, DndDropEvent, DndDropzoneDirective, DndHandleDirective, DndPlaceholderRefDirective } from "ngx-drag-drop";
import { PageHeaderComponent } from "apps/UptimeMonitor.Web.UI.Angular/src/app/layouts/admin-layout/page-header.component";

@Component({
    selector: 'app-status-page-form',
    templateUrl: 'status-page-form.component.html',
    imports: [PageHeaderComponent, ReactiveFormsModule, NgFor,
        DndDropzoneDirective, DndPlaceholderRefDirective, DndDraggableDirective, DndHandleDirective]
})
export class StatusPageFormComponent implements OnInit {
    slug: string | null = null;

    isEditMode = false;

    allMonitors: MonitorSimpleDtoPagedListResponse = {totalPages: 0, totalRecords: 0, records: []};

    form = new FormGroup({
        name: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        slug: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern('^[a-z0-9]+(?:-[a-z0-9]+)*$')] }),
        monitorGroups: new FormArray([])
    })

    get dragging(): boolean { return this.draggedType !== null; }
    draggedType: 'monitor' | 'group' | null = null;
    draggedMonitorSourceGroup: number | null = null;

    constructor(
        private readonly statusPageService: StatusPageService,
        private readonly monitorService: MonitorService,
        private readonly loadingService: LoadingService,
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.slug = this.route.snapshot.paramMap.get('slug');
        this.isEditMode = this.slug != null;

        this.loadingService.loadingOn();
        this.monitorService.getMonitors()
            .then((monitors) => {
                this.allMonitors = monitors
                if (this.slug) {
                    this.loadingService.loadingOn();
                    this.statusPageService.getStatusPage(this.slug)
                        .then(statusPage => this.populateForm(statusPage))
                        .finally(() => this.loadingService.loadingOff());
                }
            }).finally(() => this.loadingService.loadingOff());
    }

    populateForm(statusPage: StatusPageConfigResponse): void {
        this.form.patchValue({
            name: statusPage.name,
            slug: statusPage.slug
        });

        const groups = statusPage.monitorGroups.map(group => {
            const monitorsArray = new FormArray(
                group.monitors.map((monitor: MonitorSimpleDto) => new FormGroup({
                    monitorId: new FormControl(monitor.id, { nonNullable: true, validators: [Validators.required] })
                }))
            );

            return new FormGroup({
                id: new FormControl(group.id, { nonNullable: true, validators: [Validators.required] }),
                groupName: new FormControl(group.name, { nonNullable: true, validators: [Validators.required] }),
                monitors: monitorsArray
            });
        });

        this.monitorGroups.clear();
        groups.forEach(group => this.monitorGroups.push(group));
    }

    // Returns all currently selected monitor ids
    getSelectedMonitorIds(): number[] {
        const selected: number[] = [];
        this.monitorGroups.controls.forEach(group => {
            const monitors = group.get('monitors') as FormArray;
            monitors.controls.forEach(monitor => {
                const id = monitor.get('monitorId')?.value;
                if (typeof id === 'number') {
                    selected.push(id);
                }
            });
        });
        return selected;
    }

    // Returns filtered monitor options for a specific monitor FormGroup
    getFilteredMonitorOptions(currentGroupIndex: number, currentMonitorIndex: number): MonitorSimpleDto[] {
        const selectedIds = this.getSelectedMonitorIds();

        const currentMonitorId = this.getMonitors(currentGroupIndex)
            .at(currentMonitorIndex)
            .get('monitorId')?.value;

        const filtered = this.allMonitors.records.filter(option =>
            !selectedIds.includes(option.id) || option.id === currentMonitorId
        );

        return filtered;
    }

    // Getter for monitorGroups
    get monitorGroups(): FormArray {
        return this.form.get('monitorGroups') as FormArray;
    }

    // Create a new Monitor Group FormGroup
    createMonitorGroup(): FormGroup {
        return new FormGroup({
            groupName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
            monitors: new FormArray([]) // Nested FormArray for monitors
        });
    }

    // Create a new Monitor FormGroup
    createMonitor(): FormGroup {
        return new FormGroup({
            monitorId: new FormControl<number | null>(null, { nonNullable: true, validators: [Validators.required] })
        });
    }

    // Add a new monitor group
    addMonitorGroup() {
        this.monitorGroups.push(this.createMonitorGroup());
    }

    // Remove a monitor group
    removeMonitorGroup(index: number) {
        this.monitorGroups.removeAt(index);
    }

    // Get the monitors FormArray for a specific monitor group
    getMonitors(groupIndex: number): FormArray {
        return this.monitorGroups.at(groupIndex).get('monitors') as FormArray;
    }

    // Add a new monitor inside a specific monitor group
    addMonitor(groupIndex: number) {
        this.getMonitors(groupIndex).push(this.createMonitor());
    }

    // Remove a monitor from a specific monitor group
    removeMonitor(groupIndex: number, monitorIndex: number) {
        this.getMonitors(groupIndex).removeAt(monitorIndex);
    }

    onDragGroupStart() {
        this.draggedType = 'group';
    }

    onDropGroup(event: DndDropEvent) {
        let newIndex = event.index;
        const oldIndex = event.data ?? undefined;

        if (typeof newIndex === 'undefined') {
            newIndex = this.monitorGroups.length;
        }
        if(newIndex === -1) {
            newIndex = this.monitorGroups.length - 1;
        }
        
        const groupItem = this.monitorGroups.at(oldIndex);
        this.monitorGroups.removeAt(oldIndex);
        this.monitorGroups.insert(newIndex, groupItem);
    }

    onDragGroupStop() {
        this.draggedType = null;
    }

    onDragMonitorStart(sourceGroupIndex: number) {
        this.draggedType = 'monitor';
        this.draggedMonitorSourceGroup = sourceGroupIndex;
    }

    onDropMonitor(event: DndDropEvent, targetGroupIndex: number) {
        let newIndex = event.index;

        const targetGroupMonitors = this.getMonitors(targetGroupIndex);

        if (typeof newIndex === 'undefined') {
            newIndex = targetGroupMonitors.length;
        }
        if(newIndex === -1) {
            newIndex = targetGroupMonitors.length - 1;
        }

        const oldIndex = event.data ?? undefined;

        if(oldIndex === undefined || this.draggedMonitorSourceGroup === null) {
            return;
        }

        const sourceGroupMonitors = (targetGroupIndex !== this.draggedMonitorSourceGroup) ? 
            this.getMonitors(this.draggedMonitorSourceGroup) : targetGroupMonitors;
        
        const monitorItem = sourceGroupMonitors.at(oldIndex);
        sourceGroupMonitors.removeAt(oldIndex);
        targetGroupMonitors.insert(newIndex, monitorItem);
    }

    onDragMonitorEnd() {
        this.draggedMonitorSourceGroup = null;
        this.draggedType = null;
    }

    onSubmit() {
        if (this.form.invalid || !this.form.value.name || !this.form.value.slug) {
            return;
        }

        const transformedMonitorGroups = this.monitorGroups.controls.map(group => {
            const groupId = group.get('id')?.value;
            const groupName = group.get('groupName')?.value;
            const monitors = (group.get('monitors') as FormArray).controls.map(monitor =>
                Number(monitor.get('monitorId')?.value)
            );

            return {
                id: groupId,
                name: groupName,
                monitorIds: monitors
            };
        });

        if (this.isEditMode && !this.slug) {
            console.error('Slug is missing in edit mode');
            return;
        }

        const payload = {
            name: this.form.value.name,
            slug: this.form.value.slug,
            monitorGroups: transformedMonitorGroups
        };

        const operation = this.isEditMode && this.slug
            ? this.statusPageService.updateStatusPage(this.slug, payload)
            : this.statusPageService.createStatusPage(payload);

        operation.then(() => {
            this.router.navigate(['../../'], { relativeTo: this.route });
        });
    }
}