using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for FindDocumentFolders.
/// </summary>
public interface IIncidentEndpointSendListIncidents0Port
{
    Task<ApiDocuments.FindDocumentFoldersResponse> SendAsync(ApiDocuments.FindDocumentFoldersRequest request, CancellationToken token);
}

public sealed class IncidentEndpointSendListIncidents0PortAdapter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : IIncidentEndpointSendListIncidents0Port
{
    public async Task<ApiDocuments.FindDocumentFoldersResponse> SendAsync(ApiDocuments.FindDocumentFoldersRequest request, CancellationToken token)
    {
        using var rpc = client.FindDocumentFoldersAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class IncidentEndpointRequests
{
    /// <summary>
    /// Builds the CalculateFuelCard request from the current workflow state.
    /// </summary>
    public static ApiFuel.CalculateFuelCardRequest GetIncidentToCalculateFuelCard0(this ApiItsm.GetIncidentRequest request)
    {
        return new ApiFuel.CalculateFuelCardRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FuelCard = new ApiFuel.FuelCard
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                ExternalAccountId = (request.IncidentId ?? ""),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
    }
}
