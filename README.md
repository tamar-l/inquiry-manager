# 🗂️ מערכת ניהול פניות | המשרד להגנת הסביבה

מערכת לניהול פניות ובקשות המתקבלות מארגונים ועסקים, עם אפשרויות סינון, מיון, דפדוף ועדכון סטטוס.

---

## 📋 תוכן עניינים

- [סקירה כללית](#-סקירה-כללית)
- [טכנולוגיות](#-טכנולוגיות)
- [ארכיטקטורה](#-ארכיטקטורה)
- [התקנה והרצה](#-התקנה-והרצה)
- [מבנה הפרויקט](#-מבנה-הפרויקט)
- [פיצ'רים](#-פיצרים)
- [החלטות טכנולוגיות](#-החלטות-טכנולוגיות)
- [שיפורים עתידיים](#-שיפורים-עתידיים)
- [שימוש ב-AI](#-שימוש-ב-ai)

---

## 🎯 סקירה כללית

המערכת נבנתה כמענה לדרישות המטלה:
- 10,000 פניות פיקטיביות בבסיס נתונים
- REST API עם סינון, מיון, דפדוף וחיפוש
- נתונים מסכמים (Aggregations) עם Cache
- עדכון סטטוס עם טיפול מלא במקרי קצה
- ממשק Angular עם עיצוב ממשלתי RTL

---

## 🛠 טכנולוגיות

### צד שרת (Backend)
| טכנולוגיה | שימוש |
|-----------|-------|
| .NET 10 Web API | שרת REST API |
| Entity Framework Core 10 | ORM וגישה לנתונים |
| SQLite | בסיס נתונים |
| IMemoryCache | Cache לנתונים מסכמים ורשימה |
| xUnit | בדיקות אוטומטיות |

### צד לקוח (Frontend)
| טכנולוגיה | שימוש |
|-----------|-------|
| Angular 19 | Framework |
| Standalone Components | ארכיטקטורה מודרנית ללא NgModules |
| RxJS | עבודה אסינכרונית + debounce לחיפוש |

---

## 🏗 ארכיטקטורה

```
┌─────────────────────────────────────────────────┐
│                   Client (Angular)               │
│     Components → Services → HTTP                 │
└──────────────────────┬──────────────────────────┘
                       │ HTTP / REST
┌──────────────────────▼──────────────────────────┐
│                   Server (.NET 10)               │
│   Controller → CachedInquiryService (Decorator)  │
│                    ↓                             │
│              InquiryService → DbContext          │
│                    ↕ Cache                       │
└──────────────────────┬──────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────┐
│                   SQLite DB                      │
│         Inquiries (נוצר אוטומטית)               │
└─────────────────────────────────────────────────┘
```

- **הפרדת אחריות** – Controller (routing בלבד), CachedInquiryService (Cache), InquiryService (לוגיקה), DbContext (גישה לנתונים)
- **Decorator Pattern** – `CachedInquiryService` עוטף את `InquiryService` ומוסיף Cache ללא שינוי הלוגיקה
- **Dependency Injection** – כל השכבות מוזרקות דרך DI container
- **DTOs** – הפרדה בין מודל ה-DB למה שנחשף ב-API

---

## 🚀 התקנה והרצה

### דרישות מקדימות
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI 19](https://angular.dev/) (`npm install -g @angular/cli`)

### הרצת השרת
```bash
cd backend/InquiryManager.API
dotnet run
```
השרת יעלה על `http://localhost:5159`  
Swagger זמין בכתובת: `http://localhost:5159/swagger`

> בהרצה ראשונה, בסיס הנתונים נוצר אוטומטית ומתמלא ב-10,000 פניות.
> EF Core Migrations מטפל ביצירת הטבלאות אוטומטית דרך `db.Database.Migrate()` ב-`Program.cs`.

### הרצת הקליינט
```bash
cd frontend/inquiry-manager
npm install
ng serve
```
האפליקציה תעלה על `http://localhost:4200`

### הרצת בדיקות
```bash
cd backend/InquiryManager.Tests
dotnet test
```

---

## 📁 מבנה הפרויקט

```
HomeTask/
├── backend/
│   ├── InquiryManager.API/
│   │   ├── Controllers/     # endpoints בלבד
│   │   ├── Services/        # InquiryService (לוגיקה) + CachedInquiryService (Decorator)
│   │   ├── Data/            # DbContext + Seed
│   │   ├── Models/          # Inquiry, Enums
│   │   ├── DTOs/            # מה שנכנס ויוצא מה-API
│   │   └── Middleware/      # טיפול גלובלי בשגיאות
│   └── InquiryManager.Tests/
│       └── UpdateStatusTests.cs  # 11 בדיקות – InquiryService + CachedInquiryService
└── frontend/
    └── inquiry-manager/
        └── src/app/
            ├── models/      # טיפוסי TypeScript
            ├── services/    # קריאות ל-API
            └── components/
                ├── inquiry-list/          # רשימה, סינון, מיון, דפדוף
                ├── inquiry-summary/       # נתונים מסכמים
                └── update-status-modal/   # עדכון סטטוס
```

### Endpoints
| Method | Route | תיאור |
|--------|-------|-------|
| GET | `/api/inquiries` | רשימה עם pagination, filtering, sorting |
| GET | `/api/inquiries/summary` | נתונים מסכמים |
| PATCH | `/api/inquiries/{id}/status` | עדכון סטטוס |

---

## ✨ פיצ'רים

### נדרש במטלה
- ✅ 10,000 פניות (EF Core Seed + אינדקסים)
- ✅ Pagination, סינון, מיון, חיפוש – הכל בשרת
- ✅ Aggregations עם Cache + invalidation
- ✅ עדכון סטטוס עם טיפול ב-404 / 400 / 500
- ✅ ממשק Angular עם כל מצבי הטעינה/שגיאה/ריק
- ✅ בדיקות אוטומטיות (xUnit)

### תוספות
- ✅ Decorator Pattern לCache – `CachedInquiryService` עוטף את `InquiryService` ב-composition
- ✅ ErrorHandlingMiddleware – טיפול גלובלי בשגיאות
- ✅ debounce לחיפוש – לא שולח בקשה על כל הקשה
- ✅ תצוגה בעברית עם ערכים באנגלית ב-API

---

## 💡 החלטות טכנולוגיות

| החלטה | סיבה |
|--------|------|
| SQLite | הרצה מיידית ללא התקנה. מעבר ל-SQL Server אפשרי בשינוי שורה אחת |
| Pagination (לא Virtual Scrolling) | עדיף לכמויות גדולות – שולף רק 20 רשומות מה-DB, לא 10,000 |
| Decorator Pattern לCache | `CachedInquiryService` עוטף את `InquiryService` ב-composition. Cache מתבטל מיידית בכל write, TTL של 5 דקות כ-safety net |
| IMemoryCache (לא Redis) | מתאים לשרת יחיד. Redis – אם יהיה צורך ב-scale out |
| InquiryService כ-Scoped | מקבל `AppDbContext` ישירות דרך DI – פשוט ונכון. אין צורך ב-ScopeFactory |
| ללא Repository Pattern | שכבה מיותרת להיקף המשימה. EF Core הוא כבר abstraction |
| חיפוש עם LIKE ב-SQLite | Contains מתורגם ל-LIKE '%...%' שאינו מנצל אינדקסים. מגבלה ידועה של SQLite – ב-SQL Server ניתן להשתמש ב-Full-Text Search |

---

## 🔧 שיפורים עתידיים

**מעבר ל-Full-Text Search** – החיפוש הנוכחי משתמש ב-LIKE '%...%' שאינו מנצל אינדקסים ב-SQLite. במעבר ל-SQL Server ניתן להשתמש ב-Full-Text Search לביצועים טובים יותר על כמויות גדולות.

**Redis במקום IMemoryCache** – בסביבת multi-instance (scale out) יש צורך ב-Distributed Cache. המעבר ל-Redis דורש שינוי מינימלי בקוד – רק ב-`CachedInquiryService`.

---

## 🤖 שימוש ב-AI

נעשה שימוש ב-Amazon Q Developer לאורך כל הפרויקט לצורך:
- יצירת scaffold ראשוני של קבצים
- עזרה בתחביר EF Core ו-Angular
- השוואה בין גישות מימוש

כל קוד שהוגש נבדק, הובן ואושר על ידי המפתחת.

---

<div align="center">
  <sub>נבנה עבור מבדק פיתוח Full Stack | המשרד להגנת הסביבה</sub>
</div>
