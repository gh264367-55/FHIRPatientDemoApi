using FHIRPatientDemoApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var fhirBaseUrl = builder.Configuration["Fhir:BaseUrl"] ?? "https://hapi.fhir.org/baseR4";

// HttpClient points at baseR4; request paths won’t repeat /baseR4
builder.Services.AddHttpClient<IFhirClient, FhirClient>(client =>
{
    client.BaseAddress = new Uri(fhirBaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/fhir+json");
});

builder.Services.AddSingleton<PatientMapper>();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();