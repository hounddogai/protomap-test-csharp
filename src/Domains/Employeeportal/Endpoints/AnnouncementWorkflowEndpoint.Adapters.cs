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
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for NormalizeSearchIndex.
/// </summary>
public interface IAnnouncementWorkflowEndpointSendListAnnouncementWorkflows0Port
{
    Task<ApiSearch.NormalizeSearchIndexResponse> SendAsync(ApiSearch.NormalizeSearchIndexRequest request, CancellationToken token);
}

public sealed class AnnouncementWorkflowEndpointSendListAnnouncementWorkflows0PortAdapter(ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient client) : IAnnouncementWorkflowEndpointSendListAnnouncementWorkflows0Port
{
    public async Task<ApiSearch.NormalizeSearchIndexResponse> SendAsync(ApiSearch.NormalizeSearchIndexRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeSearchIndexAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
