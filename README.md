# TaskManager

TaskManager is a WPF desktop application for viewing, filtering, creating, and editing task items.
The application loads the initial task list from the JSONPlaceholder `todos` API and supports local task changes during the current application session.

## Build and Run

Requirements:

- Windows
- .NET SDK with support for `net9.0-windows`

Restore dependencies:

```powershell
dotnet restore .\TaskManager.slnx
```

Build the application:

```powershell
dotnet build .\TaskManager\TaskManager.csproj
```

Run the application:

```powershell
dotnet run --project .\TaskManager\TaskManager.csproj
```

The application uses `https://jsonplaceholder.typicode.com/todos` as the remote task source, so internet access is required for the initial task loading.

## Technologies and Packages

Technologies:

- C#
- .NET `net9.0-windows`
- WPF
- MVVM
- XAML Resource Dictionaries
- `HttpClientFactory`
- `.resx` localization

NuGet packages:

- `CommunityToolkit.Mvvm` - observable properties and relay commands
- `Microsoft.Extensions.DependencyInjection` - dependency injection
- `Microsoft.Extensions.Http` - typed `HttpClient` registration
- `Serilog` - application logging

## Implemented Features

- Loads tasks from JSONPlaceholder.
- Displays tasks in a styled WPF list.
- Supports task search by title.
- Supports filtering by completion status.
- Supports filtering by users that exist in the loaded task list.
- Shows selected task details in a side panel.
- Allows local task creation.
- Allows local task editing.
- Displays validation and operation errors through localized messages.
- Supports English and Ukrainian UI localization.
- Provides an in-window MVVM-friendly add/edit dialog overlay.
- Shows a loading spinner during task loading.
- Uses single-instance application startup protection.

## Project Structure

- `TaskManager` - WPF application, views, styles, converters, application services, and DI setup.
- `TaskManager.ViewModels` - view models, UI contracts, dialog view models, and filter models.
- `TaskManager.Core` - API client and task service logic.
- `TaskManager.Models` - shared task models, operation result model, and constants.
- `TaskManager.Resources` - localization resources and localization manager.

## Scope Notes and Possible Improvements

- Local task changes are intentionally stored in memory only, because persistent storage is outside the current application scope.
- Reloading tasks loads data from the remote API again and resets local additions/edits.
- The user filter popup can be empty when there are no loaded users.
- Automated tests are not included yet.
- `MainWindowViewModel` keeps screen coordination, filtering, and dialog orchestration together to avoid introducing extra services and models for a compact test application. If the application grows, these responsibilities can be split into dedicated child view models or services.
