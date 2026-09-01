# 🗂️ מערכת ניהול פניות | המשרד להגנת הסביבה

מערכת לניהול פניות ובקשות המתקבלות מארגונים ועסקים, עם סינון, מיון, דפדוף ועדכון סטטוס.

## 🚀 הוראות הרצה

**שרת:**
```bash
cd backend/InquiryManager.API
dotnet run
```
השרת יעלה על `http://localhost:5159` | Swagger: `http://localhost:5159/swagger`
> בהרצה ראשונה בסיס הנתונים נוצר אוטומטית ומתמלא ב-10,000 פניות.

**קליינט:**
```bash
cd frontend/inquiry-manager
npm install
ng serve
```
האפליקציה תעלה על `http://localhost:4200`

**בדיקות:**
```bash
cd backend/InquiryManager.Tests
dotnet test
```

## 🛠️ טכנולוגיות
| | |
|---|---|
| Backend | .NET 10 Web API, Entity Framework Core, SQLite |
| Cache | IMemoryCache |
| Frontend | Angular 19, Standalone Components, RxJS |
| בדיקות | xUnit |

## 📁 מבנה הפתרון
```
HomeTask/
├── backend/
│   ├── InquiryManager.API/
│   │   ├── Controllers/   # endpoints בלבד
│   │   ├── Services/      # InquiryService (לוגיקה) + CachedInquiryService (Decorator)
│   │   ├── Data/          # DbContext + Seed
│   │   ├── Models/        # Inquiry, Enums
│   │   ├── DTOs/          # מה שנכנס ויוצא מה-API
│   │   └── Middleware/    # טיפול גלובלי בשגיאות
│   └── InquiryManager.Tests/
└── frontend/
    └── inquiry-manager/
```

## 🔌 Endpoints
| Method | Route | תיאור |
|--------|-------|-------|
| GET | `/api/inquiries` | רשימה עם pagination, סינון, מיון, חיפוש |
| GET | `/api/inquiries/summary` | נתונים מסכמים |
| PATCH | `/api/inquiries/{id}/status` | עדכון סטטוס |

## 💡 החלטה טכנולוגית משמעותית
**Decorator Pattern לCache** – `CachedInquiryService` עוטף את `InquiryService` ומוסיף Cache ללא שינוי הלוגיקה העסקית. Cache מתבטל מיידית בכל write, TTL של 5 דקות כ-safety net. היתרון: מחר אפשר להחליף ל-Redis בשינוי קובץ אחד בלבד.

## 🔧 שיפור עתידי
החיפוש משתמש ב-`LIKE '%...%'` שאינו מנצל אינדקסים ב-SQLite. במעבר ל-SQL Server הייתי משתמשת ב-Full-Text Search לביצועים טובים יותר על כמויות גדולות.

## 🤖 שימוש ב-AI
נעשה שימוש ב-Amazon Q Developer ליצירת scaffold ראשוני, עזרה בתחביר EF Core ו-Angular והשוואה בין גישות מימוש. כל קוד שהוגש נבדק, הובן ואושר על ידי המפתחת.
