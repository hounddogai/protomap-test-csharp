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
using ApiAppointments = Acme.Appointments.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for SimulateManagedDevice.
/// </summary>
public interface IComplianceScanValidationEndpointSendValidateComplianceScan1Port
{
    Task<ApiDevices.SimulateManagedDeviceResponse> SendAsync(ApiDevices.SimulateManagedDeviceRequest request, CancellationToken token);
}

public sealed class ComplianceScanValidationEndpointSendValidateComplianceScan1PortAdapter(ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient client) : IComplianceScanValidationEndpointSendValidateComplianceScan1Port
{
    public async Task<ApiDevices.SimulateManagedDeviceResponse> SendAsync(ApiDevices.SimulateManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.SimulateManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ComplianceScanValidationEndpointRequests
{
    /// <summary>
    /// Builds the SuggestEntitlements request from the current workflow state.
    /// </summary>
    public static ApiLicensing.SuggestEntitlementsRequest ValidateComplianceScanToSuggestEntitlements0(this ApiDevices.ValidateComplianceScanRequest request)
    {
        return new ApiLicensing.SuggestEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestEntitlementsRequest.Types.Filter
            {
                CreatedAfter = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ComplianceScan?.Subject?.CandidateId ?? ""),
            },
        };
    }
}
