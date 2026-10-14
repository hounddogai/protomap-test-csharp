using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where CopyItineraryCode hands each record.</summary>
internal interface ICopyItineraryCodeSink
{
    Task PushAsync(CopyItineraryCodeRecord record, CancellationToken token);
}

/// <summary>What CopyItineraryCode keeps of each QuoteItinerary response.</summary>
internal sealed class CopyItineraryCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string Notes { get; init; } = "";
    public string ItineraryId { get; init; } = "";
}

/// <summary>Passes records to acme.workplaces.v1.VisitorPassLifecycleService.</summary>
internal sealed class CopyItineraryCodeVisitorPassLifecycleTarget(ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient client) : ICopyItineraryCodeSink
{
    public async Task PushAsync(CopyItineraryCodeRecord record, CancellationToken token)
    {
        await client.ReinstateVisitorPassAsync(new ApiWorkplaces.ReinstateVisitorPassRequest { Details = new ApiWorkplaces.ReinstateVisitorPassRequest.Types.Details { ReasonCode = record.ItineraryId }, Reason = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.onboarding.v1.OnboardingTaskReportingService.</summary>
internal sealed class CopyItineraryCodeOnboardingTaskReportingSink : ICopyItineraryCodeSink
{
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _client;

    public CopyItineraryCodeOnboardingTaskReportingSink(ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient client) => _client = client;

    public async Task PushAsync(CopyItineraryCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ListOnboardingTaskReportsRequest { OrderBy = record.Notes, Page = new ApiCommon.PageRequest { PageToken = record.ItineraryId } }, token, _client.ListOnboardingTaskReportsAsync);
    }
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventCalculationService.</summary>
internal sealed class CopyItineraryCodeClockEventCalculationWriter : ICopyItineraryCodeSink
{
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _client;

    public CopyItineraryCodeClockEventCalculationWriter(ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient client) => _client = client;

    public async Task PushAsync(CopyItineraryCodeRecord record, CancellationToken token)
    {
        await _client.RecalculateClockEventsAsync(new ApiTimekeeping.RecalculateClockEventsRequest { Details = new ApiTimekeeping.RecalculateClockEventsRequest.Types.Details { CurrencyCode = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.ItineraryCalculationService/QuoteItinerary and hands what it keeps to the sink of its kind.</summary>
internal static class CopyItineraryCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient(channel);
        var response = await source.QuoteItineraryAsync(new ApiTravel.QuoteItineraryRequest() { Reason = "main-copy-itinerary-code" }, cancellationToken: token);
        var record = new CopyItineraryCodeRecord
        {
            CurrencyCode = response.Breakdown.CurrencyCode,
            Notes = response.Itinerary.Notes,
            ItineraryId = response.Itinerary.ItineraryId,
        };
        var sinks = new Dictionary<string, ICopyItineraryCodeSink>
        {
            ["visitor-pass-lifecycle"] = new CopyItineraryCodeVisitorPassLifecycleTarget(new ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient(channel)),
            ["onboarding-task-reporting"] = new CopyItineraryCodeOnboardingTaskReportingSink(new ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient(channel)),
            ["clock-event-calculation"] = new CopyItineraryCodeClockEventCalculationWriter(new ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient(channel)),
        };
        await RouteCopyItineraryCodeAsync("clock-event-calculation", record, sinks, token);
    }

    private static async Task RouteCopyItineraryCodeAsync(string kind, CopyItineraryCodeRecord record, IReadOnlyDictionary<string, ICopyItineraryCodeSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
