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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShows = Acme.Shows.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

public static class PositionValidationEndpointRequests
{
    /// <summary>
    /// Builds the CloneShow request from the current workflow state.
    /// </summary>
    public static ApiShows.CloneShowRequest NormalizePositionToCloneShow1(this ApiEmployees.NormalizePositionRequest request, ApiEmployees.QuoteEmploymentContractResponse reply0)
    {
        return new ApiShows.CloneShowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Show = new ApiShows.Show
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
            },
        };
    }
}
