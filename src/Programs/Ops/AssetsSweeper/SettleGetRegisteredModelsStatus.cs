using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAnomalies = Acme.Anomalies.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiModelregistry = Acme.Modelregistry.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where SettleGetRegisteredModelsStatus hands each record.</summary>
internal interface ISettleGetRegisteredModelsStatusDestination
{
    Task SendAsync(SettleGetRegisteredModelsStatusRecord record, CancellationToken token);
}

/// <summary>What SettleGetRegisteredModelsStatus keeps of each BatchGetRegisteredModels response.</summary>
internal sealed class SettleGetRegisteredModelsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.benefits.v1.BenefitPlanService.</summary>
internal sealed class SettleGetRegisteredModelsStatusBenefitPlanWriter : ISettleGetRegisteredModelsStatusDestination
{
    private readonly ApiBenefits.BenefitPlanService.BenefitPlanServiceClient _client;

    public SettleGetRegisteredModelsStatusBenefitPlanWriter(ApiBenefits.BenefitPlanService.BenefitPlanServiceClient client) => _client = client;

    public async Task SendAsync(SettleGetRegisteredModelsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.CreateBenefitPlanRequest { BenefitPlan = new ApiBenefits.BenefitPlan { Name = record.Status }, RequestId = record.NextPageToken }, token, _client.CreateBenefitPlanAsync);
    }
}

/// <summary>Hands records to acme.anomalies.v1.AnomalyService.</summary>
internal sealed class SettleGetRegisteredModelsStatusAnomalyRelay : ISettleGetRegisteredModelsStatusDestination
{
    private readonly ApiAnomalies.AnomalyService.AnomalyServiceClient _client;

    public SettleGetRegisteredModelsStatusAnomalyRelay(ApiAnomalies.AnomalyService.AnomalyServiceClient client) => _client = client;

    public async Task SendAsync(SettleGetRegisteredModelsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAnomalies.CreateAnomalyRequest { RequestId = record.NextPageToken, Anomaly = new ApiAnomalies.Anomaly { Subject = record.Message } }, token, _client.CreateAnomalyAsync);
    }
}

/// <summary>Reads acme.modelregistry.v1.RegisteredModelService/BatchGetRegisteredModels and hands what it keeps to the sink of its kind.</summary>
internal static class SettleGetRegisteredModelsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient(channel);
        var response = await source.BatchGetRegisteredModelsAsync(new ApiModelregistry.BatchGetRegisteredModelsRequest(), cancellationToken: token);
        var record = new SettleGetRegisteredModelsStatusRecord
        {
            Status = response.Result.Status,
            NextPageToken = response.PageInfo.NextPageToken,
            Message = response.Result.Message,
        };
        var sinks = new Dictionary<string, ISettleGetRegisteredModelsStatusDestination>
        {
            ["benefit-plan"] = new SettleGetRegisteredModelsStatusBenefitPlanWriter(new ApiBenefits.BenefitPlanService.BenefitPlanServiceClient(channel)),
            ["anomaly"] = new SettleGetRegisteredModelsStatusAnomalyRelay(new ApiAnomalies.AnomalyService.AnomalyServiceClient(channel)),
        };
        await RouteSettleGetRegisteredModelsStatusAsync("benefit-plan", record, sinks, token);
    }

    private static async Task RouteSettleGetRegisteredModelsStatusAsync(string kind, SettleGetRegisteredModelsStatusRecord record, IReadOnlyDictionary<string, ISettleGetRegisteredModelsStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
