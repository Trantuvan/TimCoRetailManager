# TimCoRetailManager
## _Retail Management system


TimCoRetailManager is a desktop-app, offline-storage compatible,
Blazor-WebAssembly which is a Progressive web application.

- This project focuses on real-world development. As such, simulating that works for TimCo Enterprise Solutions on a brand new product, the TimCo Retail Manager. Just like in the real world, starting out with one set of requirements but know that over time they will change.
- ✨Magic ✨

## Features

- Web API as a portal to power 2 UI (Desktop-app & Blazor WebAssembly) throught AspNetCore HttpClient
- Desktop-app is a cash register and role managing system
- Blazor-WebAssembly is inventory report and managing-system
- Seperation of concerns Databases (ApiAuthDB & TRMData)
- ApiAuthDB as default EF core database to managing authentication services
- TRMData as local database to store relative data that needed for the project

## Tech

TimCoRetailManager uses a number of notable nuggetpackages:

- [Swagger] - Display and testing API
- [Entity FrameWork] - Managing Authentication database
- [Dapper] - Managing project database
- [Tokens.JWT] - Generate JwT token
- [Blazored.LocalStorage] - Caching data working with Progressive Web App
- [AutoMapper] - fast mapping between models
- [Caliburn.Mirco] - Configuring, and implementing **SimpleContainer** for Desktop-app
- [HttpClient] - Passing data between Class Libraries

## Installation

TimCoRetailManager requires [Microsoft.NET.Sdk] net5.0 and net5.0-windows to run.

**Create TRMData**:
- Right Click TRMData project folder to publish
- Edit publish Database
- Click Browse tab & Selecting right Local Database
- Click Save Profile and Publish

**Create ApiAuthDB**:
- Make Sure **TRMApi** as Startup project
- Open Package Manager and run ```update-database```

Run **TRMApi** and Click _Privacy_ to generate roles: 
-Username: "Trantuvan.kan@gmail.com"
-Password: "Pwd12345."
-Passing id of AspNetUser table in **ApiAuthDB** to User table in **TRMData**

## License

MIT

**Free Software, Hell Yeah!**
