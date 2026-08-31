import { Component, OnInit, Output, EventEmitter, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, catchError, of, finalize } from 'rxjs';
import { InquiryService } from '../../services/inquiry.service';
import { Inquiry, InquiryQueryParams, PagedResult, UpdateStatusRequest, INQUIRY_STATUSES, INQUIRY_PRIORITIES } from '../../models/inquiry.model';
import { UpdateStatusModalComponent } from '../update-status-modal/update-status-modal.component';

@Component({
  selector: 'app-inquiry-list',
  standalone: true,
  imports: [CommonModule, FormsModule, UpdateStatusModalComponent],
  templateUrl: './inquiry-list.component.html',
  styles: []
})
export class InquiryListComponent implements OnInit {
  private readonly service = inject(InquiryService);
  private readonly searchSubject = new Subject<string>();
  private readonly destroyRef = inject(DestroyRef);

  @Output() statusUpdated = new EventEmitter<void>();

  result: PagedResult<Inquiry> | null = null;
  loading = true;
  error = false;
  search = '';

  params: InquiryQueryParams = { page: 1, pageSize: 20, sortBy: 'createdAt', sortDesc: false, status: null, priority: null };

  selectedInquiry: Inquiry | null = null;
  modalLoading = false;
  modalError: string | null = null;

  readonly statuses = INQUIRY_STATUSES;
  readonly priorities = INQUIRY_PRIORITIES;

  statusLabel(status: string)    { return this.statuses.find(s => s.value === status)?.label ?? status; }
  priorityLabel(priority: string) { return this.priorities.find(p => p.value === priority)?.label ?? priority; }

  readonly columns = [
    { key: 'id',               label: 'מזהה' },
    { key: 'title',            label: 'כותרת' },
    { key: 'organizationName', label: 'ארגון' },
    { key: 'status',           label: 'סטטוס' },
    { key: 'priority',         label: 'עדיפות' },
    { key: 'createdAt',        label: 'תאריך יצירה' },
  ];

  get totalPages() { return Math.ceil((this.result?.totalCount ?? 0) / this.params.pageSize!); }

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(search => {
      this.params = { ...this.params, search, page: 1 };
      this.load();
    });
    this.load();
  }

  load() {
    this.loading = true;
    this.error = false;
    this.service.getInquiries(this.params).subscribe({
      next: data => { this.result = data; this.loading = false; },
      error: () => { this.error = true; this.loading = false; }
    });
  }

  onSearchChange(value: string) { this.searchSubject.next(value); }
  onFilterChange() { this.params = { ...this.params, page: 1 }; this.load(); }
  onPageChange(page: number) { this.params = { ...this.params, page }; this.load(); }
  onSort(key: string) {
    this.params = { ...this.params, sortBy: key, sortDesc: this.params.sortBy === key ? !this.params.sortDesc : false, page: 1 };
    this.load();
  }

  openModal(inquiry: Inquiry) {
    this.selectedInquiry = inquiry;
    this.modalError = null;
  }

  onConfirmStatus(request: UpdateStatusRequest) {
    if (!this.selectedInquiry) return;
    this.modalLoading = true;
    this.modalError = null;

    this.service.updateStatus(this.selectedInquiry.id, request).pipe(
      catchError(err => {
        this.modalError = err?.error?.error ?? 'שגיאה בעדכון הסטטוס.';
        return of(null);
      }),
      finalize(() => this.modalLoading = false)
    ).subscribe(updated => {
      if (updated) {
        this.result = {
          ...this.result!,
          items: this.result!.items.map(i => i.id === updated.id ? { ...i, status: updated.status, updatedAt: updated.updatedAt } : i)
        };
        this.selectedInquiry = null;
        this.statusUpdated.emit();
      }
    });
  }
}
