# Tell Teddie

Public project snapshot for Tell Teddie, a full-stack .NET application for sharing short-lived audio and text posts.

Live site: https://tellteddie.com

## Git history is limited in this repository

This GitHub repository is a public-facing sanitised mirror of the project.

The original development workflow was conducted in **Azure DevOps**, where I used:

- Boards / agile planning / sprints
- Pull requests and branch policies
- CI/CD pipelines
- Automated testing
- Repository hosting

Before publishing this project publicly, I rewrote the Git history and removed sensitive configuration so the code could be shared safely. As a result, this repository does not reflect the full internal commit history, pull request trail, or delivery timeline from the original Azure DevOps environment.

This GitHub repo is a **clean public snapshot**, not the complete development record.

## What tellteddie.com is

Tell Teddie is a web application where users can post, with captions, anonomously through the following mediums:

- Audio recordings 
- Text posts

Posts are designed to be temporary. Expired content is removed automatically after 24 hours, including associated audio files stored in blob storage.

Tell Teddie was created after I lost my companion Teddie, who listened to me when I wanted to share the good and the bad with her. I wanted to recreate this experience for myself and for others whilst honouring her memory.

## Tech Stack

- .NET 8
- Blazor Server
- ASP.NET Core Web API
- SQL Server
- Dapper
- Azure Blob Storage
- Bootstrap
- WaveSurfer.js

## Architecture Overview

The solution is split into separate projects by responsibility:

- **TellTeddie.Web**  
  Blazor Server frontend. Handles UI rendering, user interaction, and calls into the API.

- **TellTeddie.Api**  
  Backend API layer. Exposes endpoints for retrieving posts, creating text/audio posts, and handling audio upload workflows.

- **TellTeddie.Core**  
  Domain models representing the core business entities.

- **TellTeddie.Contracts**  
  Shared DTOs and contract types used between layers.

- **TellTeddie.Infrastructure**  
  Data access and external integrations, including SQL repositories and Azure Blob Storage operations.

- **Test projects**  
  Separate test projects exist for API, Core, Infrastructure, and Web concerns.

## Design Approach

A few implementation choices I would highlight:

- **Layered architecture**  
  The solution separates UI, API, domain, contracts, and infrastructure concerns rather than collapsing everything into one project.

- **Repository-based data access with Dapper**  
  I used Dapper instead of a heavier ORM to keep SQL explicit and easier to keep track of in git.

- **Blob storage for audio**  
  Audio files are stored outside the relational database, in Azure Blob Storage which keeps the data model cleaner and scales better for media content.

- **Background cleanup process**  
  A hosted service periodically removes expired posts and deletes related blobs, so temporary content behaves as intended. Posts over 24 hours are also filtered out using SQL.

- **Dependency injection throughout**  
  Services and repositories are wired through interfaces to keep the code testable and replaceable.

## Current State

This project demonstrates:

- A working full-stack .NET application
- Aeparation of concerns across multiple projects
- Media upload handling
- Database-backed feed retrieval
- Automated cleanup of expired content
- Automated tests

The API currently leans more toward an RPC-style route design than a fully RESTful resource design, which is one of the areas I would improve next.

## Future Plans

Current backlog contains features & PBIs for:

- Move project to GitHub completely!
- Refactor API routes toward more RESTful resource naming
- Refactor Controllers to use Service layer for API calls
- Refactor .razor views - move code out of @code block in razor page
- Add authentication / authorisation
- Replace stock UI design with one created in Figma (designs available on request)
- Improve validation and error handling
- Improve feed query efficiency by refactoring multi-query get from DB
- Strengthen automated test coverage
- Continue evolving the UI and posting experience
- Add image and video post functionality
- Reconsider potential uses for the application

