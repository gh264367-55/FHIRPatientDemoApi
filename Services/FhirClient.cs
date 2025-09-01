using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FhirPatientApi.Models;

namespace FHIRPatientDemoApi.Services
{
    public sealed class FhirClient : IFhirClient
    {
        private readonly HttpClient _http;
        private readonly PatientMapper _mapper;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FhirClient(HttpClient http, PatientMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<PatientSummary?> GetPatientByIdAsync(string id, CancellationToken ct = default)
        {
            // GET [base]/Patient/{id}
            var res = await _http.GetAsync($"/baseR4/Patient/{Uri.EscapeDataString(id)}", ct);
            if (!res.IsSuccessStatusCode) return null;

            using var stream = await res.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            // Map a single Patient resource
            return _mapper.MapPatient(doc.RootElement);
        }

        public async Task<IReadOnlyList<PatientSummary>> SearchPatientsAsync(
            string? name = null,
            string? identifier = null,
            string? birthDate = null,
            int count = 20,
            int page = 1,
            CancellationToken ct = default)
        {
            // FHIR search: GET [base]/Patient?name=smith&identifier=...&_count=...&_getpagesoffset=...
            var qs = new StringBuilder("/baseR4/Patient?");
            if (!string.IsNullOrWhiteSpace(name)) qs.Append($"name={Uri.EscapeDataString(name)}&");
            if (!string.IsNullOrWhiteSpace(identifier)) qs.Append($"identifier={Uri.EscapeDataString(identifier)}&");
            if (!string.IsNullOrWhiteSpace(birthDate)) qs.Append($"birthdate={Uri.EscapeDataString(birthDate)}&");
            qs.Append($"_count={count}&");

            // HAPI supports _getpagesoffset (offset-based) & _count; page -> offset
            var offset = Math.Max(0, (page - 1) * count);
            qs.Append($"_getpagesoffset={offset}");

            var res = await _http.GetAsync(qs.ToString(), ct);
            res.EnsureSuccessStatusCode();

            using var stream = await res.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            // Expect a Bundle with entry[].resource (type Patient)
            var root = doc.RootElement;
            var list = new List<PatientSummary>();

            if (root.TryGetProperty("entry", out var entries) && entries.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    if (!entry.TryGetProperty("resource", out var resource)) continue;
                    var type = resource.GetProperty("resourceType").GetString();
                    if (type?.Equals("Patient", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var ps = _mapper.MapPatient(resource);
                        if (ps is not null) list.Add(ps);
                    }
                }
            }

            return list;
        }
    }
}
