# החלטות טכנולוגיות – מערכת ניהול פניות

## מבנה הפרויקט
```
HomeTask/
├── backend/
│   ├── InquiryManager.API/
│   └── InquiryManager.Tests/
└── frontend/
    └── inquiry-manager/
```

## טכנולוגיות
- **Backend:** .NET 10 Web API
- **ORM:** Entity Framework Core
- **DB:** SQLite (קל להרצה, ללא התקנה. מעבר ל-SQL Server אפשרי בשינוי שורה אחת)
- **Frontend:** Angular 19 (Standalone Components)
- **אתגר:** In-Memory Cache (IMemoryCache של .NET, מתאים לשרת יחיד. Redis – אם יהיה צורך ב-scale out)
- **בדיקה:** בדיקת עדכון סטטוס – כולל מקרי קצה (פנייה לא קיימת, סטטוס לא תקין)

## מבנה ה-API
```
backend/InquiryManager.API/
├── Models/Inquiry.cs
├── Data/AppDbContext.cs
├── DTOs/InquiryDto.cs + InquiryQueryParams.cs
├── Services/InquiryService.cs
└── Controllers/InquiriesController.cs
```
