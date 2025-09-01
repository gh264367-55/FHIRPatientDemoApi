using System.Text.Json;
using FhirPatientApi.Models;

namespace FHIRPatientDemoApi.Services
{
    public sealed class PatientMapper
    {
        public PatientSummary? MapPatient(JsonElement patient)
        {
            if (patient.ValueKind == JsonValueKind.Undefined || patient.ValueKind == JsonValueKind.Null)
                return null;

            string? id = TryGetString(patient, "id");
            string? gender = TryGetString(patient, "gender");
            string? birthDate = TryGetString(patient, "birthDate");

            // name[0] -> text OR (given + family)
            string? displayName = null;
            if (patient.TryGetProperty("name", out var names) && names.ValueKind == JsonValueKind.Array)
            {
                var first = names.EnumerateArray().FirstOrDefault();
                if (first.ValueKind != JsonValueKind.Undefined)
                {
                    displayName = TryGetString(first, "text");
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        var given = first.TryGetProperty("given", out var givenArr) && givenArr.ValueKind == JsonValueKind.Array
                            ? string.Join(" ", givenArr.EnumerateArray().Select(g => g.GetString()).Where(s => !string.IsNullOrWhiteSpace(s)))
                            : null;
                        var family = TryGetString(first, "family");
                        displayName = string.Join(" ", new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    }
                }
            }

            // identifier[0].value (MRN example – depends on system)
            string? mrn = null;
            if (patient.TryGetProperty("identifier", out var identifiers) && identifiers.ValueKind == JsonValueKind.Array)
            {
                foreach (var idObj in identifiers.EnumerateArray())
                {
                    var system = TryGetString(idObj, "system");
                    var value = TryGetString(idObj, "value");
                    // Heuristic: if system hints at MRN or local hospital system, prefer it
                    if (!string.IsNullOrWhiteSpace(value) &&
                        (system?.Contains("mrn", StringComparison.OrdinalIgnoreCase) == true ||
                         system?.Contains("medicalrecord", StringComparison.OrdinalIgnoreCase) == true ||
                         system?.Contains("hospital", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        mrn = value;
                        break;
                    }
                    mrn ??= value; // fallback to first identifier value
                }
            }

            // telecom – pick first phone/email
            string? phone = null, email = null;
            if (patient.TryGetProperty("telecom", out var telecoms) && telecoms.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in telecoms.EnumerateArray())
                {
                    var system = TryGetString(t, "system");
                    var value = TryGetString(t, "value");
                    if (string.Equals(system, "phone", StringComparison.OrdinalIgnoreCase) && phone is null)
                        phone = value;
                    if (string.Equals(system, "email", StringComparison.OrdinalIgnoreCase) && email is null)
                        email = value;
                }
            }

            // address[0] -> line[], city, state, postalCode, country
            string? address = null;
            if (patient.TryGetProperty("address", out var addresses) && addresses.ValueKind == JsonValueKind.Array)
            {
                var a0 = addresses.EnumerateArray().FirstOrDefault();
                if (a0.ValueKind != JsonValueKind.Undefined)
                {
                    var parts = new List<string>();
                    if (a0.TryGetProperty("line", out var lines) && lines.ValueKind == JsonValueKind.Array)
                        parts.AddRange(lines.EnumerateArray().Select(l => l.GetString()).Where(s => !string.IsNullOrWhiteSpace(s))!);

                    foreach (var key in new[] { "city", "state", "postalCode", "country" })
                    {
                        var val = TryGetString(a0, key);
                        if (!string.IsNullOrWhiteSpace(val)) parts.Add(val);
                    }

                    address = string.Join(", ", parts);
                }
            }

            return new PatientSummary
            {
                Id = id,
                MRN = mrn,
                Name = string.IsNullOrWhiteSpace(displayName) ? null : displayName,
                Gender = gender,
                BirthDate = birthDate,
                Phone = phone,
                Email = email,
                Address = address
            };
        }

        private static string? TryGetString(JsonElement obj, string propName)
            => obj.TryGetProperty(propName, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    }
}
