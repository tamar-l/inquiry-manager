import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InquiryService } from '../../services/inquiry.service';
import { InquirySummary } from '../../models/inquiry.model';

@Component({
  selector: 'app-inquiry-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inquiry-summary.component.html',
  styles: []
})
export class InquirySummaryComponent implements OnInit {
  private readonly service = inject(InquiryService);

  summary: InquirySummary | null = null;
  loading = true;
  error = false;

  get statusEntries() { return Object.entries(this.summary?.byStatus ?? {}); }
  get priorityEntries() { return Object.entries(this.summary?.byPriority ?? {}); }

  private readonly statusMap: Record<string, string> = { New: 'חדש', InProgress: 'בטיפול', Waiting: 'ממתין', Completed: 'הושלם' };
  private readonly priorityMap: Record<string, string> = { Low: 'נמוכה', Medium: 'בינונית', High: 'גבוהה' };

  translateStatus(key: string) { return this.statusMap[key] ?? key; }
  translatePriority(key: string) { return this.priorityMap[key] ?? key; }

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = false;
    this.service.getSummary().subscribe({
      next: data => { this.summary = data; this.loading = false; },
      error: () => { this.error = true; this.loading = false; }
    });
  }
}
