using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardOnboardingPlanNotes hands each record.</summary>
internal interface IForwardOnboardingPlanNotesDestination
{
    Task DeliverAsync(ForwardOnboardingPlanNotesRecord record, CancellationToken token);
}

/// <summary>What ForwardOnboardingPlanNotes keeps of each PeekOnboardingPlan response.</summary>
internal sealed class ForwardOnboardingPlanNotesRecord
{
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ServiceAssetApprovalService.</summary>
internal sealed class ForwardOnboardingPlanNotesServiceAssetApprovalSink : IForwardOnboardingPlanNotesDestination
{
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _client;

    public ForwardOnboardingPlanNotesServiceAssetApprovalSink(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(ForwardOnboardingPlanNotesRecord record, CancellationToken token)
    {
        await _client.ListPendingServiceAssetsAsync(new ApiItsm.ListPendingServiceAssetsRequest { Filter = new ApiItsm.ListPendingServiceAssetsRequest.Types.Filter { OwnerId = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentFolderValidationService.</summary>
internal sealed class ForwardOnboardingPlanNotesDocumentFolderValidationRelay : IForwardOnboardingPlanNotesDestination
{
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _client;

    public ForwardOnboardingPlanNotesDocumentFolderValidationRelay(ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(ForwardOnboardingPlanNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.NormalizeDocumentFolderRequest { Reason = record.Status }, token, _client.NormalizeDocumentFolderAsync);
    }
}

/// <summary>Passes records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class ForwardOnboardingPlanNotesCalibrationValidationSink : IForwardOnboardingPlanNotesDestination
{
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _client;

    public ForwardOnboardingPlanNotesCalibrationValidationSink(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(ForwardOnboardingPlanNotesRecord record, CancellationToken token)
    {
        await _client.LintCalibrationsAsync(new ApiPerformance.LintCalibrationsRequest { Calibration = new ApiPerformance.Calibration { Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingPlanQueryService/PeekOnboardingPlan and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardOnboardingPlanNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient(channel);
        var reply = await source.PeekOnboardingPlanAsync(new ApiOnboarding.PeekOnboardingPlanRequest() { OnboardingPlanId = "main-forward-onboarding-plan-notes" }, cancellationToken: token);
        var record = new ForwardOnboardingPlanNotesRecord
        {
            Notes = reply.OnboardingPlan.Notes,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, IForwardOnboardingPlanNotesDestination>
        {
            ["asset-approval"] = new ForwardOnboardingPlanNotesServiceAssetApprovalSink(new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(channel)),
            ["document-folder-validation"] = new ForwardOnboardingPlanNotesDocumentFolderValidationRelay(new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(channel)),
            ["calibration-validation"] = new ForwardOnboardingPlanNotesCalibrationValidationSink(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel)),
        };
        await RouteForwardOnboardingPlanNotesAsync("asset-approval", record, sinks, token);
    }

    private static async Task RouteForwardOnboardingPlanNotesAsync(string kind, ForwardOnboardingPlanNotesRecord record, IReadOnlyDictionary<string, IForwardOnboardingPlanNotesDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
