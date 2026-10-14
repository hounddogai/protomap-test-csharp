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
using ApiDispatch = Acme.Dispatch.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public static class LearningPathQueryEndpointRequests
{
    /// <summary>
    /// Builds the CalculateLearningPath request from the current workflow state.
    /// </summary>
    public static ApiLearning.CalculateLearningPathRequest SummarizeLearningPathsToCalculateLearningPath1(this ApiLearning.SummarizeLearningPathsRequest request, ApiLearning.CompareCertificationOptionsResponse reply0)
    {
        return new ApiLearning.CalculateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.Certification?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply0.Certification?.BillingAddress?.CountryCode ?? ""),
                            Region = (reply0.Certification?.BillingAddress?.Region ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Locality = (reply0.Certification?.BillingAddress?.Locality ?? ""),
                    },
                } },
            },
        };
    }
}
