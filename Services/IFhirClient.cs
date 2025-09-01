namespace FHIRPatientDemoApi.Services
{
    using FhirPatientApi.Models;
    using global::FhirPatientApi.Models;
       

    public interface IFhirClient
    {
        Task<PatientSummary?> GetPatientByIdAsync(string id, CancellationToken ct = default);
        Task<IReadOnlyList<PatientSummary>> SearchPatientsAsync(
            string? name = null,
            string? identifier = null,
            string? birthDate = null, // yyyy-mm-dd
            int count = 20,
            int page = 1,
            CancellationToken ct = default);
    }
}
