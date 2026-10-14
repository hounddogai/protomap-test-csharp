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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for FindTravelBookings.
/// </summary>
public interface ICourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port
{
    Task<ApiTravel.FindTravelBookingsResponse> SendAsync(ApiTravel.FindTravelBookingsRequest request, CancellationToken token);
}

public sealed class CourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0PortAdapter(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : ICourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port
{
    public async Task<ApiTravel.FindTravelBookingsResponse> SendAsync(ApiTravel.FindTravelBookingsRequest request, CancellationToken token)
    {
        using var rpc = client.FindTravelBookingsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CourseEnrollmentValidationEndpointRequests
{
    /// <summary>
    /// Builds the ValidateLearningPath request from the current workflow state.
    /// </summary>
    public static ApiLearning.ValidateLearningPathRequest ExplainCourseEnrollmentDecisionToValidateLearningPath0(this ApiLearning.ExplainCourseEnrollmentDecisionRequest request)
    {
        return new ApiLearning.ValidateLearningPathRequest
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
                            UpdatedBy = (request.CourseEnrollment?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Contact?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
    }
}
