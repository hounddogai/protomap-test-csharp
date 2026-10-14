using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiLearning = Acme.Learning.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where PropagateBuildingName hands each record.</summary>
internal interface IPropagateBuildingNameWriter
{
    Task ForwardAsync(PropagateBuildingNameRecord record, CancellationToken token);
}

/// <summary>What PropagateBuildingName keeps of each CloneBuilding response.</summary>
internal sealed class PropagateBuildingNameRecord
{
    public string Name { get; init; } = "";
    public string BuildingId { get; init; } = "";
}

/// <summary>Sends records to acme.learning.v1.CertificationWorkflowService.</summary>
internal sealed class PropagateBuildingNameCertificationWorkflowWriter : IPropagateBuildingNameWriter
{
    private readonly ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient _client;

    public PropagateBuildingNameCertificationWorkflowWriter(ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(PropagateBuildingNameRecord record, CancellationToken token)
    {
        await _client.InspectCertificationWorkflowAsync(new ApiLearning.InspectCertificationWorkflowRequest { CertificationId = record.BuildingId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.VenueReportingService.</summary>
internal sealed class PropagateBuildingNameVenueReportingRelay(ApiCorporateevents.VenueReportingService.VenueReportingServiceClient client) : IPropagateBuildingNameWriter
{
    public async Task ForwardAsync(PropagateBuildingNameRecord record, CancellationToken token)
    {
        await client.DeleteVenueReportAsync(new ApiCorporateevents.DeleteVenueReportRequest { VenueId = record.Name }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.BuildingService/CloneBuilding and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateBuildingName
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.BuildingService.BuildingServiceClient(channel);
        var response = await source.CloneBuildingAsync(new ApiWorkplaces.CloneBuildingRequest() { RequestId = "main-propagate-building-name" }, cancellationToken: token);
        var record = new PropagateBuildingNameRecord
        {
            Name = response.Building.Name,
            BuildingId = Text.Normalize(response.Building.BuildingId),
        };
        await ActOnPropagateBuildingNameAsync("venue-reporting", record, channel, token);
    }

    private static async Task ActOnPropagateBuildingNameAsync(string kind, PropagateBuildingNameRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["certification-workflow"] = () => new PropagateBuildingNameCertificationWorkflowWriter(new ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient(channel)).ForwardAsync(record, token),
            ["venue-reporting"] = () => new PropagateBuildingNameVenueReportingRelay(new ApiCorporateevents.VenueReportingService.VenueReportingServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
