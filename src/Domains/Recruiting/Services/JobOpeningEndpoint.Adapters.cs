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
using ApiAudit = Acme.Audit.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public static class JobOpeningEndpointRequests
{
    /// <summary>
    /// Builds the FindCandidates request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.FindCandidatesRequest UpdateJobOpeningToFindCandidates0(this ApiRecruiting.UpdateJobOpeningRequest request)
    {
        return new ApiRecruiting.FindCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.FindCandidatesRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
