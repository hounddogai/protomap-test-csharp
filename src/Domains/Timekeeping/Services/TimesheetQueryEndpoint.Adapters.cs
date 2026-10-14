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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for PreviewCourseEnrollment.
/// </summary>
public interface ITimesheetQueryEndpointSendSummarizeTimesheets1Port
{
    Task<ApiLearning.PreviewCourseEnrollmentResponse> SendAsync(ApiLearning.PreviewCourseEnrollmentRequest request, CancellationToken token);
}

public sealed class TimesheetQueryEndpointSendSummarizeTimesheets1PortAdapter(ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient client) : ITimesheetQueryEndpointSendSummarizeTimesheets1Port
{
    public async Task<ApiLearning.PreviewCourseEnrollmentResponse> SendAsync(ApiLearning.PreviewCourseEnrollmentRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewCourseEnrollmentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
