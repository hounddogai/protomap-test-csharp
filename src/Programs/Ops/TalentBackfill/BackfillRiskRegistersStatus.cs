using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiRisk = Acme.Risk.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What BackfillRiskRegistersStatus keeps of each ListRiskRegisters response.</summary>
internal sealed class BackfillRiskRegistersStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.workplaces.v1.VisitorPassCalculationService.</summary>
internal sealed class BackfillRiskRegistersStatusVisitorPassCalculationTarget(ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient client)
{
    public async Task ApplyAsync(BackfillRiskRegistersStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.ExplainVisitorPassCalculationRequest { VisitorPass = new ApiWorkplaces.VisitorPass { VisitorPassId = record.Status }, VisitorPassId = record.Message }, token, client.ExplainVisitorPassCalculationAsync);
    }
}

/// <summary>Reads acme.risk.v1.RiskRegisterService/ListRiskRegisters and hands what it keeps to acme.workplaces.v1.VisitorPassCalculationService.</summary>
internal static class BackfillRiskRegistersStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRisk.RiskRegisterService.RiskRegisterServiceClient(channel);
        var response = await source.ListRiskRegistersAsync(new ApiRisk.ListRiskRegistersRequest() { OrderBy = "current-backfill-risk-registers-status" }, cancellationToken: token);
        var record = new BackfillRiskRegistersStatusRecord
        {
            Status = response.Result.Status,
            Message = response.Result.Message,
        };
        await new BackfillRiskRegistersStatusVisitorPassCalculationTarget(new ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient(channel)).ApplyAsync(record, token);
    }
}
