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
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelBookingReportingEndpoint
{
    /// <summary>
    /// Counts travel bookings by status.
    /// </summary>
    public override async Task<ApiTravel.CountTravelBookingTotalsResponse> CountTravelBookingTotals(ApiTravel.CountTravelBookingTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountTravelBookingTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "CountTravelBookingTotals", "accepted", token);
        var input0 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        using var rpc0 = _sendCountTravelBookingTotals0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CountTravelBookingTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "CountTravelBookingTotals", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a travel booking.
    /// </summary>
    public override async Task<ApiTravel.DeleteTravelBookingReportResponse> DeleteTravelBookingReport(ApiTravel.DeleteTravelBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling DeleteTravelBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "DeleteTravelBookingReport", "accepted", token);
        var input0 = new ApiCorporateevents.EscalateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.EscalateRegistrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient>("acme.corporateevents.v1.RegistrationApprovalService");
        using var rpc0 = named0.EscalateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        var descriptor1 = new Method<ApiEmployees.SearchEmployeesRequest, ApiEmployees.SearchEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeService", "SearchEmployees",
            Wire.Marshaller<ApiEmployees.SearchEmployeesRequest>(), Wire.Marshaller<ApiEmployees.SearchEmployeesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    Token = (reply0.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Card => (reply0.Registration?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.DeleteTravelBookingReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "DeleteTravelBookingReport", (request.TravelBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.FetchTravelBookingReportResponse> FetchTravelBookingReport(ApiTravel.FetchTravelBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling FetchTravelBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "FetchTravelBookingReport", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.FetchTravelBookingReportRequest, ApiTravel.ExplainTravelBookingCalculationRequest>(value => new ApiTravel.ExplainTravelBookingCalculationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTravelBookingCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (value.TravelBookingId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelTravelBookingCalculationService.ExplainTravelBookingCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.FetchTravelBookingReportResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                Status = (reply0.TravelBooking?.Status ?? (ApiTravel.TravelBookingStatus)0),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.GenerateTravelBookingReportResponse> GenerateTravelBookingReport(ApiTravel.GenerateTravelBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateTravelBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "GenerateTravelBookingReport", "accepted", token);
        var input0 = new ApiHelpdesk.CancelKnowledgeArticleWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CancelKnowledgeArticleWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Attributes = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _helpdeskKnowledgeArticleWorkflowService.CancelKnowledgeArticleWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.GenerateTravelBookingReportToResolveAuditEvents1();
        using var rpc1 = _auditAuditEventQueryService.ResolveAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.GenerateTravelBookingReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "GenerateTravelBookingReport", (request.TravelBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ScheduleTravelBookingReportResponse> ScheduleTravelBookingReport(ApiTravel.ScheduleTravelBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleTravelBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "ScheduleTravelBookingReport", "accepted", token);
        var input0 = new ApiTravel.ListTravelPolicyConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListTravelPolicyConflictsRequest.Types.Filter
            {
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelBookingId ?? ""),
            },
        };
        var reply0 = await SendScheduleTravelBookingReport0Async(input0, token);
        var input1 = new ApiDocuments.ShareSignatureExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsSignatureExportService.ShareSignatureExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScheduleTravelBookingReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Signature?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ScheduleTravelBookingReport", (request.Details?.Format ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ShareTravelBookingReportResponse> ShareTravelBookingReport(ApiTravel.ShareTravelBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareTravelBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "ShareTravelBookingReport", "accepted", token);
        var input0 = new ApiTravel.CheckTripEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CheckTripEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.CheckTripEligibilityRequest>();
        using var rpc0 = _travelTripValidationService.CheckTripEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.LintDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    Bic = (reply0.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Bank => (reply0.Trip?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _documentsDocumentValidationService.LintDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ShareTravelBookingReportResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Document?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Notes = (reply0.Trip?.Notes ?? ""),
                Number = (reply0.Trip?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ShareTravelBookingReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of travel bookings as they happen.
    /// </summary>
    public override async Task StreamTravelBookingMetrics(ApiTravel.StreamTravelBookingMetricsRequest request, IServerStreamWriter<ApiTravel.StreamTravelBookingMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamTravelBookingMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "StreamTravelBookingMetrics", "accepted", token);
        var input0 = new ApiDevices.RejectManagedDeviceRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDevices.RejectManagedDeviceRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                Email = (request.Subscriber?.Email ?? ""),
            },
        };
        input0.ManagedDevice = new ApiDevices.ManagedDevice
        {
            Address = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _devicesManagedDeviceApprovalService.RejectManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _documentsSignatureSyncService.SyncSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.StreamTravelBookingMetricsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Number = (reply1.Signature?.Subject?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "StreamTravelBookingMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiTravel.ListTravelPolicyConflictsResponse> SendScheduleTravelBookingReport0Async(ApiTravel.ListTravelPolicyConflictsRequest request, CancellationToken token)
    {
        return await _travelBookingReportingEndpointSendScheduleTravelBookingReport0Port.SendAsync(request, token);
    }
}
