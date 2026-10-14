using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where SettleProblemCode hands each record.</summary>
internal interface ISettleProblemCodeDestination
{
    Task ApplyAsync(SettleProblemCodeRecord record, CancellationToken token);
}

/// <summary>What SettleProblemCode keeps of each SimulateProblem response.</summary>
internal sealed class SettleProblemCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string ProblemId { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassApprovalService.</summary>
internal sealed class SettleProblemCodeVisitorPassApprovalWriter : ISettleProblemCodeDestination
{
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _client;

    public SettleProblemCodeVisitorPassApprovalWriter(ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleProblemCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.CommentVisitorPassApprovalRequest { VisitorPass = new ApiWorkplaces.VisitorPass { Notes = record.Subject, VisitorPassId = record.ProblemId }, VisitorPassId = record.CurrencyCode }, token, _client.CommentVisitorPassApprovalAsync);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentService.</summary>
internal sealed class SettleProblemCodeDocumentSink(ApiDocuments.DocumentService.DocumentServiceClient client) : ISettleProblemCodeDestination
{
    public async Task ApplyAsync(SettleProblemCodeRecord record, CancellationToken token)
    {
        await client.CreateDocumentAsync(new ApiDocuments.CreateDocumentRequest { Document = new ApiDocuments.Document { DocumentId = record.CurrencyCode, Notes = record.Subject }, RequestId = record.ProblemId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleProblemCodeLogOnly : ISettleProblemCodeDestination
{
    public Task ApplyAsync(SettleProblemCodeRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CurrencyCode}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.itsm.v1.ProblemCalculationService/SimulateProblem and hands what it keeps to the sink of its kind.</summary>
internal static class SettleProblemCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient(channel);
        var got = await source.SimulateProblemAsync(new ApiItsm.SimulateProblemRequest() { ProblemId = "nightly-settle-problem-code" }, cancellationToken: token);
        var record = new SettleProblemCodeRecord
        {
            CurrencyCode = got.Breakdown.CurrencyCode,
            ProblemId = Text.Normalize(got.Problem.ProblemId),
            Subject = Text.Normalize(got.Problem.Subject),
        };
        await DispatchSettleProblemCodeAsync("visitor-pass-approval", record, channel, token);
    }

    private static async Task DispatchSettleProblemCodeAsync(string kind, SettleProblemCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SettleProblemCodeRecord, CancellationToken, Task> write = new SettleProblemCodeLogOnly().ApplyAsync;
        switch (kind)
        {
            case "visitor-pass-approval":
                write = new SettleProblemCodeVisitorPassApprovalWriter(new ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient(channel)).ApplyAsync;
                break;
            case "document":
                write = new SettleProblemCodeDocumentSink(new ApiDocuments.DocumentService.DocumentServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
