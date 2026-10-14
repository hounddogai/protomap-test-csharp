using Grpc.Net.Client;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>What SettleUserStatus keeps of each NormalizeUser response.</summary>
internal sealed class SettleUserStatusRecord
{
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ProblemCalculationService.</summary>
internal sealed class SettleUserStatusProblemCalculationTarget(ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient client)
{
    public async Task WriteAsync(SettleUserStatusRecord record, CancellationToken token)
    {
        await client.GetProblemRatesAsync(new ApiItsm.GetProblemRatesRequest { ProblemId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.identity.v1.UserValidationService/NormalizeUser and hands what it keeps to acme.itsm.v1.ProblemCalculationService.</summary>
internal static class SettleUserStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiIdentity.UserValidationService.UserValidationServiceClient(channel);
        var found = await source.NormalizeUserAsync(new ApiIdentity.NormalizeUserRequest() { Reason = "live-settle-user-status" }, cancellationToken: token);
        var record = new SettleUserStatusRecord
        {
            Status = found.Result.Status,
            OutcomeStatus = found.Outcome.Status,
            Message = found.Result.Message,
            CorrelationId = found.Result.CorrelationId,
        };
        await new SettleUserStatusProblemCalculationTarget(new ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient(channel)).WriteAsync(record, token);
    }
}
