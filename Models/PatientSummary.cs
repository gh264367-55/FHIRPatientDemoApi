namespace FhirPatientApi.Models
{
    public record PatientSummary
    {
        public string? Id { get; init; }
        public string? MRN { get; init; }                // Identifier (example)
        public string? Name { get; init; }               // Display name
        public string? Gender { get; init; }
        public string? BirthDate { get; init; }          // yyyy-mm-dd
        public string? Phone { get; init; }              // First telecom phone
        public string? Email { get; init; }              // First telecom email
        public string? Address { get; init; }            // Single-line formatted
    }
}
