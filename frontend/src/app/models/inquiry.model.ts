export type InquiryStatus = 'New' | 'InProgress' | 'Waiting' | 'Completed';
export type InquiryPriority = 'Low' | 'Medium' | 'High';

export const INQUIRY_STATUSES = [
  { value: 'New' as InquiryStatus,        label: 'חדש' },
  { value: 'InProgress' as InquiryStatus, label: 'בטיפול' },
  { value: 'Waiting' as InquiryStatus,    label: 'ממתין' },
  { value: 'Completed' as InquiryStatus,  label: 'הושלם' },
];

export const INQUIRY_PRIORITIES = [
  { value: 'Low' as InquiryPriority,    label: 'נמוכה' },
  { value: 'Medium' as InquiryPriority, label: 'בינונית' },
  { value: 'High' as InquiryPriority,   label: 'גבוהה' },
];

export interface Inquiry {
  id: number;
  title: string;
  organizationName: string;
  status: InquiryStatus;
  priority: InquiryPriority;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface InquirySummary {
  total: number;
  byStatus: Record<string, number>;
  byPriority: Record<string, number>;
}

export interface InquiryQueryParams {
  search?: string;
  organizationName?: string | null;
  status?: InquiryStatus | null;
  priority?: InquiryPriority | null;
  sortBy?: string;
  sortDesc?: boolean;
  page?: number;
  pageSize?: number;
}

export interface UpdateStatusRequest {
  status: string;
}
