import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
    selector: 'lib-paginator',
    templateUrl: 'paginator.component.html'
})
export class PaginatorComponent {
    @Input({ required: true }) pageCount!: number;
    @Input({ required: true }) pageSize!: number;
    @Input() currentPage = 1;

    @Output() pageChanged = new EventEmitter<number>();

    get visiblePages(): number[] {
        const range = 2;
        const start = Math.max(1, this.currentPage - range);
        const end = Math.min(this.pageCount, this.currentPage + range);
        const pages: number[] = [];

        for (let i = start; i <= end; i++) {
            pages.push(i);
        }

        return pages;
    }
}