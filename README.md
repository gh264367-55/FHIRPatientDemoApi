# FHIRPatientDemoApi
ASP.NET Core Web API that fetches patient demographics from a FHIR server (HAPI FHIR R4). Demonstrates healthcare integration with .NET

Features
Search for patients by name, identifier, or birthdate
Fetch a single patient by ID
Returns a simplified PatientSummary model (ID, MRN, Name, Gender, BirthDate, Phone, Email, Address)
Built using .NET 8, HttpClient, and System.Text.Json
Auto-generated Swagger UI for testing

Project Structure
FHIRPatientDemoApi/
├─ Controllers/
│  └─ PatientsController.cs     # API endpoints
├─ Services/
│  ├─ IFhirClient.cs            # Interface for FHIR client
│  ├─ FhirClient.cs             # Implementation using HttpClient
│  └─ PatientMapper.cs          # Maps raw FHIR JSON to PatientSummary
├─ Models/
│  └─ PatientSummary.cs         # Simplified patient demographics model
├─ appsettings.json             # Config (FHIR Base URL)
├─ Program.cs                   # Startup / dependency injection
└─ FHIRPatientDemoApi.csproj

Tech Stack
.NET 8.0 (ASP.NET Core Web API)
HttpClient for REST calls
System.Text.Json for JSON parsing
Swagger / OpenAPI for testing endpoints

Uses public HAPI FHIR server, so data may change or reset.
For reliable demos, consider running a local HAPI FHIR server or using Azure API for FHIR.
No external libraries required.
