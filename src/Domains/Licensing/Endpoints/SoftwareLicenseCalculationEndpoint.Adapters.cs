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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiContent = Acme.Content.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Licensing;

public static class SoftwareLicenseCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ResolveSeatMaps request from the current workflow state.
    /// </summary>
    public static ApiSeating.ResolveSeatMapsRequest QuoteSoftwareLicenseToResolveSeatMaps0(this ApiLicensing.QuoteSoftwareLicenseRequest request)
    {
        return new ApiSeating.ResolveSeatMapsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
