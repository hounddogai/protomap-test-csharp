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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

public static class CandidateValidationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainJobOpeningDecision request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.ExplainJobOpeningDecisionRequest CheckCandidateEligibilityToExplainJobOpeningDecision1(this ApiRecruiting.CheckCandidateEligibilityRequest request)
    {
        return new ApiRecruiting.ExplainJobOpeningDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainJobOpeningDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Candidate?.PrimaryAddress?.CountryCode ?? ""),
                    Recipient = (request.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
    }
}
