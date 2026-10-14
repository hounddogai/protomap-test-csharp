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
using ApiDrivers = Acme.Drivers.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public static class BuildingQueryEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeDocuments request from the current workflow state.
    /// </summary>
    public static ApiDocuments.SummarizeDocumentsRequest SummarizeBuildingsToSummarizeDocuments2(this ApiWorkplaces.SummarizeBuildingsRequest request)
    {
        return new ApiDocuments.SummarizeDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SummarizeDocumentsRequest.Types.Details
            {
                Filters = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RecipientEmails = { (request.Details?.RecipientEmails ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
    }
}
