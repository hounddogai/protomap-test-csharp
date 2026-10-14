using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCustomers = Acme.Customers.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>What PushCustomersStatus keeps of each SearchCustomers response.</summary>
internal sealed class PushCustomersStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.boxoffice.v1.BoxOfficeSaleService.</summary>
internal sealed class PushCustomersStatusBoxOfficeSaleWriter(ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient client)
{
    public async Task DeliverAsync(PushCustomersStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBoxoffice.CloneBoxOfficeSaleRequest { BoxOfficeSale = new ApiBoxoffice.BoxOfficeSale { Notes = record.Message, Number = record.NextPageToken, BoxOfficeSaleId = record.Status } }, token, client.CloneBoxOfficeSaleAsync);
    }
}

/// <summary>Reads acme.customers.v1.CustomerService/SearchCustomers and hands what it keeps to acme.boxoffice.v1.BoxOfficeSaleService.</summary>
internal static class PushCustomersStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCustomers.CustomerService.CustomerServiceClient(channel);
        var response = await source.SearchCustomersAsync(new ApiCustomers.SearchCustomersRequest() { OrderBy = "live-push-customers-status" }, cancellationToken: token);
        var record = new PushCustomersStatusRecord
        {
            Status = Text.Normalize(response.Result.Status),
            Message = Text.Normalize(response.Result.Message),
            NextPageToken = response.PageInfo.NextPageToken,
        };
        await new PushCustomersStatusBoxOfficeSaleWriter(new ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient(channel)).DeliverAsync(record, token);
    }
}
