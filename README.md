# HustleLink

HustleLink is an ASP.NET Core Razor Pages app for discovering independent creators through short videos of their work. Creator profiles, videos, and follows are stored in PostgreSQL.

## Requirements

- .NET 10 SDK
- PostgreSQL 15 or later

## Run locally

1. Start a local PostgreSQL database using Docker Compose:

   ```sh
   export POSTGRES_PASSWORD='choose-a-local-password'
   docker compose up -d db
   ```

2. Set the app connection string to the same local database credentials:

   ```sh
   export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=hustlelink;Username=hustlelink;Password=$POSTGRES_PASSWORD"
   ```

3. Start the app:

   ```sh
   dotnet run --project src/HustleLink
   ```

The app creates the initial tables and sample creator/video content when the database is empty. Configure a production database connection through environment variables or a secret store; do not commit credentials.

Stop the local database with `docker compose down`. Add `-v` only if you also want to delete its stored database data.

## Current scope

- Razor Pages rendered on the server; no separate HTML, JavaScript, or CSS frontend.
- PostgreSQL-backed creator profiles, video posts, and following relationships.
- Discover and Following feeds, with server-side Follow/Unfollow forms.

The sample video and image URLs are demonstration content; replace them with your own uploaded media before production use.
