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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for FetchLicenseAssignmentReport.
/// </summary>
public interface ILicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port
{
    Task<ApiLicensing.FetchLicenseAssignmentReportResponse> SendAsync(ApiLicensing.FetchLicenseAssignmentReportRequest request, CancellationToken token);
}

public sealed class LicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3PortAdapter(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : ILicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port
{
    public async Task<ApiLicensing.FetchLicenseAssignmentReportResponse> SendAsync(ApiLicensing.FetchLicenseAssignmentReportRequest request, CancellationToken token)
    {
        using var rpc = client.FetchLicenseAssignmentReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LicenseAssignmentWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeOnboardingPlans request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.SummarizeOnboardingPlansRequest CancelLicenseAssignmentWorkflowToSummarizeOnboardingPlans2(this ApiLicensing.CancelLicenseAssignmentWorkflowRequest request, ApiHelpdesk.ExplainEscalationCalculationResponse reply0)
    {
        return new ApiOnboarding.SummarizeOnboardingPlansRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SummarizeOnboardingPlansRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
    }
}
