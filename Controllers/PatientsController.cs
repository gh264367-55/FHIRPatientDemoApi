using FhirPatientApi.Models;
using FHIRPatientDemoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHIRPatientDemoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IFhirClient _fhir;

        public PatientsController(IFhirClient fhir)
        {
            _fhir = fhir;
        }

        // GET /api/patients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientSummary>> GetById(string id, CancellationToken ct)
        {
            var result = await _fhir.GetPatientByIdAsync(id, ct);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // GET /api/patients?name=smith&identifier=12345&birthDate=1980-01-15&_count=20&page=1
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PatientSummary>>> Search(
            [FromQuery] string? name,
            [FromQuery] string? identifier,
            [FromQuery] string? birthDate,
            [FromQuery(Name = "_count")] int count = 20,
            [FromQuery] int page = 1,
            CancellationToken ct = default)
        {
            if (count <= 0 || count > 100) count = 20;
            if (page <= 0) page = 1;

            var results = await _fhir.SearchPatientsAsync(name, identifier, birthDate, count, page, ct);
            return Ok(results);
        }
    }
}
