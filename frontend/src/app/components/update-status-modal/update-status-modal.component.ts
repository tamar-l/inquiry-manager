import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Inquiry, InquiryStatus, UpdateStatusRequest, INQUIRY_STATUSES } from '../../models/inquiry.model';

@Component({
  selector: 'app-update-status-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-backdrop" (click)="cancel.emit()">
      <div class="modal-box" (click)="$event.stopPropagation()">
        <h3>עדכון סטטוס – פנייה #{{ inquiry?.id }}</h3>
        <p class="modal-subtitle">{{ inquiry?.title }} · {{ inquiry?.organizationName }}</p>

        <label>סטטוס חדש</label>
        <select [(ngModel)]="newStatus">
          @for (s of statuses; track s.value) {
            <option [value]="s.value">{{ s.label }}</option>
          }
        </select>

        @if (error) {
          <p class="modal-error">{{ error }}</p>
        }

        <div class="modal-actions">
          <button class="btn-secondary" (click)="cancel.emit()">ביטול</button>
          <button class="btn-primary" [disabled]="loading" (click)="submit()">
            {{ loading ? 'שומר...' : 'שמור' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop { position:fixed; inset:0; background:rgba(0,0,0,.45); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal-box { background:#fff; border-radius:8px; padding:24px; width:400px; max-width:95vw; display:flex; flex-direction:column; gap:10px; direction:rtl; }
    h3 { margin:0; font-size:17px; color:var(--color-heading); }
    .modal-subtitle { margin:0; color:var(--color-text-secondary); font-size:13px; }
    label { font-size:13px; font-weight:600; color:var(--color-text); margin-bottom:-6px; }
    select { border:1px solid var(--color-border); border-radius:4px; padding:7px 10px; font-size:14px; font-family:var(--font); width:100%; box-sizing:border-box; }
    select:focus { border-color:var(--color-primary); outline:none; }
    .modal-actions { display:flex; justify-content:flex-start; gap:8px; margin-top:4px; }
    .btn-primary { background:var(--color-primary); color:#fff; border:none; border-radius:4px; padding:8px 20px; cursor:pointer; font-size:14px; font-family:var(--font); }
    .btn-primary:disabled { opacity:.5; cursor:not-allowed; }
    .btn-secondary { background:#fff; border:1px solid var(--color-border); border-radius:4px; padding:8px 20px; cursor:pointer; font-size:14px; font-family:var(--font); }
    .modal-error { color:var(--color-danger); font-size:13px; margin:0; }
  `]
})
export class UpdateStatusModalComponent implements OnChanges {
  @Input() inquiry: Inquiry | null = null;
  @Input() loading = false;
  @Input() error: string | null = null;
  @Output() confirm = new EventEmitter<UpdateStatusRequest>();
  @Output() cancel = new EventEmitter<void>();

  readonly statuses = INQUIRY_STATUSES;
  newStatus: InquiryStatus = 'New';

  ngOnChanges() {
    if (this.inquiry) this.newStatus = this.inquiry.status;
  }

  submit() {
    this.confirm.emit({ status: this.newStatus });
  }
}
