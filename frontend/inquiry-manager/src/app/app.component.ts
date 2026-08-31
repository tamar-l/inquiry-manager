import { Component } from '@angular/core';
import { InquirySummaryComponent } from './components/inquiry-summary/inquiry-summary.component';
import { InquiryListComponent } from './components/inquiry-list/inquiry-list.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [InquirySummaryComponent, InquiryListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {}
