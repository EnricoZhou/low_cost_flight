# ✈️ Low Cost Flight Finder

Applicazione Full-Stack per la ricerca e il monitoraggio di offerte voli low-cost.

---

## 🛠️ Tech Stack

- **Backend:** .NET 10, ASP.NET Core, FastEndpoints (REPR), Entity Framework Core 10
- **Database & Cache:** SQL Server 2022, Redis 7 (Distributed Cache con TTL)
- **Frontend:** Angular, TypeScript, Orval (generazione automatica client API)
- **Servizi Esterni:** SerpApi (Google Flights Deals engine)
- **Audit:** Audit.NET

---

## 📁 Struttura

```text
low-cost-flight/
├── backend/            # Web API (.NET 10)
├── frontend/           # Client SPA (Angular)
├── docker-compose.yaml # SQL Server e Redis
├── .env                # Credenziali locali
└── README.md
```

---

## 🚀 Avvio Rapido

### 1. Avvia Database e Redis
```powershell
docker compose up -d
```

### 2. Avvia Backend (.NET)
```powershell
cd backend
dotnet watch
```
* **Swagger:** http://localhost:5273/swagger

### 3. Avvia Frontend (Angular)
```powershell
cd frontend
npm install
npm start
```
* **App Web:** http://localhost:4200

---

## 🔄 Sincronizzazione API (Orval)

Se modifichi gli endpoint nel backend, aggiorna i client TypeScript con:

```powershell
cd frontend
npm run generate-api
```
