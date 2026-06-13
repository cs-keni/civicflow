# CivicFlow Screenshots

## How to Capture

1. Start the app: `docker compose up --build` from the repo root
2. Open `http://localhost:5000`
3. Log in as Admin (`admin1@civicflow.dev` / `CivicFlow@2026!`)
4. Capture the pages listed below
5. Save as `.jpg` in this directory (1280×800 or 1440×900 recommended)
6. Commit: `git add docs/screenshots/ && git commit -m "docs: add portfolio screenshots"`

## Pages to Capture

| Filename | URL | Notes |
|---|---|---|
| `login.jpg` | `/login` | Show demo credentials hint visible |
| `dashboard-admin.jpg` | `/dashboard` | Logged in as Admin — all stat cards visible |
| `permits-list.jpg` | `/permits` | Show mixed statuses and pagination |
| `permit-new-wizard.jpg` | `/permits/new` (step 2) | Show AI suggestions panel loaded |
| `permit-detail.jpg` | `/permits/{id}` | Pick an "Under Review" permit — show review actions |
| `inspection-detail.jpg` | `/inspections/{id}` | Pick a Completed inspection — show AI summary card |
| `public-search.jpg` | `/public/search` | Not logged in — show search with results |
