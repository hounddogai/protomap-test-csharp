using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPatientapp = Acme.Patientapp.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PropagatePrescriptionRatesStatus hands each record.</summary>
internal interface IPropagatePrescriptionRatesStatusDestination
{
    Task ApplyAsync(PropagatePrescriptionRatesStatusRecord record, CancellationToken token);
}

/// <summary>What PropagatePrescriptionRatesStatus keeps of each GetPrescriptionRates response.</summary>
internal sealed class PropagatePrescriptionRatesStatusRecord
{
    public string Status { get; init; } = "";
    public string PrescriptionId { get; init; } = "";
}

/// <summary>Writes records to acme.patientapp.v1.PatientHomeQueryService.</summary>
internal sealed class PropagatePrescriptionRatesStatusPatientHomeQueryRelay : IPropagatePrescriptionRatesStatusDestination
{
    private readonly ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient _client;

    public PropagatePrescriptionRatesStatusPatientHomeQueryRelay(ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagatePrescriptionRatesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPatientapp.BrowsePatientHomesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiPatientapp.BrowsePatientHomesRequest.Types.Filter { OwnerId = record.PrescriptionId } }, token, _client.BrowsePatientHomesAsync);
    }
}

/// <summary>Sends records to acme.webbff.v1.PageCompositionQueryService.</summary>
internal sealed class PropagatePrescriptionRatesStatusPageCompositionQuerySink : IPropagatePrescriptionRatesStatusDestination
{
    private readonly ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient _client;

    public PropagatePrescriptionRatesStatusPageCompositionQuerySink(ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagatePrescriptionRatesStatusRecord record, CancellationToken token)
    {
        await _client.SuggestPageCompositionsAsync(new ApiWebbff.SuggestPageCompositionsRequest { Filter = new ApiWebbff.SuggestPageCompositionsRequest.Types.Filter { OwnerId = record.PrescriptionId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentCalculationService.</summary>
internal sealed class PropagatePrescriptionRatesStatusCourseEnrollmentCalculationTarget(ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient client) : IPropagatePrescriptionRatesStatusDestination
{
    public async Task ApplyAsync(PropagatePrescriptionRatesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.ExplainCourseEnrollmentCalculationRequest { CourseEnrollmentId = record.Status, CourseEnrollment = new ApiLearning.CourseEnrollment { CourseEnrollmentId = record.PrescriptionId } }, token, client.ExplainCourseEnrollmentCalculationAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagatePrescriptionRatesStatusLogOnly : IPropagatePrescriptionRatesStatusDestination
{
    public Task ApplyAsync(PropagatePrescriptionRatesStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.prescriptions.v1.PrescriptionCalculationService/GetPrescriptionRates and hands what it keeps to the sink of its kind.</summary>
internal static class PropagatePrescriptionRatesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient(channel);
        var response = await source.GetPrescriptionRatesAsync(new ApiPrescriptions.GetPrescriptionRatesRequest() { PrescriptionId = "live-propagate-prescription-rates-status" }, cancellationToken: token);
        var record = new PropagatePrescriptionRatesStatusRecord
        {
            Status = response.Result.Status,
            PrescriptionId = response.Prescription.PrescriptionId,
        };
        await DeliverPropagatePrescriptionRatesStatusAsync("page-composition-query", record, channel, token);
    }

    private static async Task DeliverPropagatePrescriptionRatesStatusAsync(string kind, PropagatePrescriptionRatesStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagatePrescriptionRatesStatusDestination sink = kind switch
        {
            "patient-home-query" => new PropagatePrescriptionRatesStatusPatientHomeQueryRelay(new ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient(channel)),
            "page-composition-query" => new PropagatePrescriptionRatesStatusPageCompositionQuerySink(new ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient(channel)),
            "course-enrollment-calculation" => new PropagatePrescriptionRatesStatusCourseEnrollmentCalculationTarget(new ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient(channel)),
            _ => new PropagatePrescriptionRatesStatusLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
