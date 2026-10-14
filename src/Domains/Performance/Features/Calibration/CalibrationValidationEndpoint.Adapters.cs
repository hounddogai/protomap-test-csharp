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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for EstimateCalibration.
/// </summary>
public interface ICalibrationValidationEndpointSendCheckCalibrationEligibility1Port
{
    Task<ApiPerformance.EstimateCalibrationResponse> SendAsync(ApiPerformance.EstimateCalibrationRequest request, CancellationToken token);
}

public sealed class CalibrationValidationEndpointSendCheckCalibrationEligibility1PortAdapter(ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient client) : ICalibrationValidationEndpointSendCheckCalibrationEligibility1Port
{
    public async Task<ApiPerformance.EstimateCalibrationResponse> SendAsync(ApiPerformance.EstimateCalibrationRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateCalibrationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CalibrationValidationEndpointRequests
{
    /// <summary>
    /// Builds the GetCalibrationRates request from the current workflow state.
    /// </summary>
    public static ApiPerformance.GetCalibrationRatesRequest CheckCalibrationEligibilityToGetCalibrationRates0(this ApiPerformance.CheckCalibrationEligibilityRequest request)
    {
        return new ApiPerformance.GetCalibrationRatesRequest
        {
            CalibrationId = (request.Calibration?.CalibrationId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
