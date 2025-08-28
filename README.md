# Last.fm Music Library

A web application built with ASP.NET Core 9.0 and Blazor Server that integrates with the Last.fm API to import and display music data.


Technologies Used


Backend: ASP.NET Core 9.0
Frontend: Blazor Server
Database: SQL Server (Entity Framework Core)
API: Last.fm Web API



## Installation


1. Clone the repository

```bash
git clone https://github.com/mtabassum/LastFmApp.git
```
   
2. Navigate to the project directory:


```bash
cd LastFmApp
```
   

1. Get Last.fm API Credentials
   

   Create a Last.fm account at https://www.last.fm
   Get an API key from https://www.last.fm/api/account/create
   Note down your API Key and Secret
   

4. Configure the application

   The repository includes an appsettings.json.example file as a template. Rename it to appsettings.json.
   Then edit appsettings.json with your actual values:

   {
   "ConnectionStrings": {
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LastFmApp;Trusted\_Connection=True;TrustServerCertificate=True"
   },
   "LastFm": {
   "ApiKey": "your\_actual\_api\_key\_here",
   "ApiSecret": "your\_actual\_api\_secret\_here"
   }
   }

   Important:

   -Replace <your\_actual\_api\_key\_here> with your actual Last.fm API key
   -Replace <your\_actual\_api\_secret\_here> with your actual API secret
   -Update the Server in ConnectionStrings if not using LocalDB
   

5. Set up the database

   Navigate to the web project directory

   cd LastFmApp.Web

   Run migrations

   dotnet ef database update

   

6. Run the application

   dotnet run
