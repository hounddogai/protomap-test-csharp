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
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public static class FeedbackWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PreviewCandidate request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.PreviewCandidateRequest SignalFeedbackWorkflowToPreviewCandidate1(this ApiPerformance.SignalFeedbackWorkflowRequest request, ApiPerformance.ResumeReviewWorkflowResponse reply0)
    {
        return new ApiRecruiting.PreviewCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                NationalId = new ApiRecruiting.CandidateNationalId
                {
                    Number = (reply0.Review?.Number ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Review?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
