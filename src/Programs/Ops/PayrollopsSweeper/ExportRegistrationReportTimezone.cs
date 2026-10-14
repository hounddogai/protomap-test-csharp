using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ExportRegistrationReportTimezone hands each record.</summary>
internal interface IExportRegistrationReportTimezoneTarget
{
    Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token);
}

/// <summary>What ExportRegistrationReportTimezone keeps of each ScheduleRegistrationReport response.</summary>
internal sealed class ExportRegistrationReportTimezoneRecord
{
    public string Timezone { get; init; } = "";
    public string DownloadUri { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ProblemCalculationService.</summary>
internal sealed class ExportRegistrationReportTimezoneProblemCalculationRelay(ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient client) : IExportRegistrationReportTimezoneTarget
{
    public async Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token)
    {
        await client.CompareProblemOptionsAsync(new ApiItsm.CompareProblemOptionsRequest { Reason = record.DownloadUri }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.concessions.v1.ConcessionStandService.</summary>
internal sealed class ExportRegistrationReportTimezoneConcessionStandRelay(ApiConcessions.ConcessionStandService.ConcessionStandServiceClient client) : IExportRegistrationReportTimezoneTarget
{
    public async Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token)
    {
        await client.UpdateConcessionStandAsync(new ApiConcessions.UpdateConcessionStandRequest { ConcessionStand = new ApiConcessions.ConcessionStand { ConcessionStandId = record.DownloadUri, Name = record.Timezone } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingTaskApprovalService.</summary>
internal sealed class ExportRegistrationReportTimezoneOnboardingTaskApprovalRelay(ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient client) : IExportRegistrationReportTimezoneTarget
{
    public async Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.RejectOnboardingTaskRequest { Reason = record.Timezone, OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.DownloadUri } }, token, client.RejectOnboardingTaskAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.EquipmentCalculationService.</summary>
internal sealed class ExportRegistrationReportTimezoneEquipmentCalculationWriter(ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient client) : IExportRegistrationReportTimezoneTarget
{
    public async Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token)
    {
        await client.EstimateEquipmentAsync(new ApiOnboarding.EstimateEquipmentRequest { Details = new ApiOnboarding.EstimateEquipmentRequest.Types.Details { CurrencyCode = record.Timezone } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ExportRegistrationReportTimezoneLogOnly : IExportRegistrationReportTimezoneTarget
{
    public Task PushAsync(ExportRegistrationReportTimezoneRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Timezone}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationReportingService/ScheduleRegistrationReport and hands what it keeps to the sink of its kind.</summary>
internal static class ExportRegistrationReportTimezone
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient(channel);
        var found = await source.ScheduleRegistrationReportAsync(new ApiCorporateevents.ScheduleRegistrationReportRequest() { Reason = "live-export-registration-report-timezone" }, cancellationToken: token);
        var record = new ExportRegistrationReportTimezoneRecord
        {
            Timezone = found.Period.Timezone,
            DownloadUri = found.DownloadUri,
        };
        await DispatchExportRegistrationReportTimezoneAsync("problem-calculation", record, channel, token);
    }

    private static async Task DispatchExportRegistrationReportTimezoneAsync(string kind, ExportRegistrationReportTimezoneRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ExportRegistrationReportTimezoneRecord, CancellationToken, Task> write = new ExportRegistrationReportTimezoneLogOnly().PushAsync;
        switch (kind)
        {
            case "problem-calculation":
                write = new ExportRegistrationReportTimezoneProblemCalculationRelay(new ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient(channel)).PushAsync;
                break;
            case "concession-stand":
                write = new ExportRegistrationReportTimezoneConcessionStandRelay(new ApiConcessions.ConcessionStandService.ConcessionStandServiceClient(channel)).PushAsync;
                break;
            case "onboarding-task-approval":
                write = new ExportRegistrationReportTimezoneOnboardingTaskApprovalRelay(new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(channel)).PushAsync;
                break;
            case "equipment-calculation":
                write = new ExportRegistrationReportTimezoneEquipmentCalculationWriter(new ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
