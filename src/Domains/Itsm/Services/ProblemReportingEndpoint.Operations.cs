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
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ProblemReportingEndpoint
{
    /// <summary>
    /// Deletes a problem.
    /// </summary>
    public override async Task<ApiItsm.DeleteProblemReportResponse> DeleteProblemReport(ApiItsm.DeleteProblemReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling DeleteProblemReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "DeleteProblemReport", "accepted", token);
        var input0 = new ApiItsm.StreamChangeRequestMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _itsmChangeRequestReportingService.StreamChangeRequestMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamChangeRequestMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.GenerateServiceAssetReportRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiItsm.GenerateServiceAssetReportRequest>();
        using var rpc1 = _itsmServiceAssetReportingService.GenerateServiceAssetReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.EstimateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiItsm.EstimateChangeRequestRequest>(json2);
        using var rpc2 = _itsmChangeRequestCalculationService.EstimateChangeRequestAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiReturns.SuggestReturnRequestsRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Filter = new ApiReturns.SuggestReturnRequestsRequest.Types.Filter
        {
            CreatedAfter = (reply2.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
            Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (reply2.Result?.CorrelationId ?? ""),
        };
        using var rpc3 = _returnsReturnRequestQueryService.SuggestReturnRequestsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.DeleteProblemReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ProblemId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "DeleteProblemReport", (request.ProblemId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.FetchProblemReportResponse> FetchProblemReport(ApiItsm.FetchProblemReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling FetchProblemReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "FetchProblemReport", "accepted", token);
        var input0 = new ApiItsm.NormalizeChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.NormalizeChangeRequestRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _itsmChangeRequestValidationService.NormalizeChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPricing.SummarizePriceListsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPricing.SummarizePriceListsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _pricingPriceListQueryService.SummarizePriceListsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.FetchProblemReportResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a problem.
    /// </summary>
    public override async Task<ApiItsm.GenerateProblemReportResponse> GenerateProblemReport(ApiItsm.GenerateProblemReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateProblemReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GenerateProblemReport", "accepted", token);
        var input0 = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseWorkflowService.CancelSoftwareLicenseWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateManagedDeviceRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                    Name = (reply0.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.SoftwareLicense?.Contact?.Phone ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync);
        var input2 = new ApiItsm.CompareServiceAssetOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ManagedDevice?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc2 = _itsmServiceAssetCalculationService.CompareServiceAssetOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.GenerateProblemReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ProblemId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "GenerateProblemReport", (request.ProblemId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists problems a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListProblemReportsResponse> ListProblemReports(ApiItsm.ListProblemReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListProblemReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", "batch",
            "ListProblemReports", "accepted", token);
        var response = new ApiItsm.ListProblemReportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ListProblemReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a problem.
    /// </summary>
    public override async Task<ApiItsm.ScheduleProblemReportResponse> ScheduleProblemReport(ApiItsm.ScheduleProblemReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleProblemReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "ScheduleProblemReport", "accepted", token);
        var input0 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc0 = named0.CalculateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.NormalizeIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiItsm.NormalizeIncidentRequest, ApiItsm.NormalizeIncidentResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".IncidentValidationService", "NormalizeIncident",
            Wire.Marshaller<ApiItsm.NormalizeIncidentRequest>(), Wire.Marshaller<ApiItsm.NormalizeIncidentResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ScheduleProblemReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ProblemId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ScheduleProblemReport", (request.ProblemId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a problem.
    /// </summary>
    public override async Task<ApiItsm.ShareProblemReportResponse> ShareProblemReport(ApiItsm.ShareProblemReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareProblemReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "ShareProblemReport", "accepted", token);
        var input0 = new ApiDocuments.ValidateDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Recipient?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.ValidateDocumentFolderRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentFolderValidationService.ValidateDocumentFolderAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ListServiceAssetReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListServiceAssetReportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _itsmServiceAssetReportingService.ListServiceAssetReportsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiGiftcards.CreateGiftCardRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            GiftCard = new ApiGiftcards.GiftCard
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.DocumentFolder?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _giftcardsGiftCardService.CreateGiftCardAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = request.ShareProblemReportToNormalizeIncident3(reply0, reply2);
        using var rpc3 = _itsmIncidentValidationService.NormalizeIncidentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ShareProblemReportResponse
        {
            Outcome = new ApiItsm.ShareProblemReportResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply3.Incident?.Audit?.CreatedBy ?? ""),
                    Etag = (reply2.GiftCard?.Audit?.Etag ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.GiftCard?.Contact?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ShareProblemReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of problems as they happen.
    /// </summary>
    public override async Task StreamProblemMetrics(ApiItsm.StreamProblemMetricsRequest request, IServerStreamWriter<ApiItsm.StreamProblemMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamProblemMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", "batch",
            "StreamProblemMetrics", "accepted", token);
        var input0 = new ApiDevices.RejectManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RejectManagedDeviceRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendStreamProblemMetrics0Async(input0, token);
        var input1 = new ApiEmployees.FindEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindEmployeesRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _employeesEmployeeQueryService.FindEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.EscalateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Dimensions = new ApiDevices.ManagedDeviceDimensions
                {
                    Length = (reply0.ManagedDevice?.Dimensions?.Length ?? 0d),
                },
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiDevices.EscalateManagedDeviceRequest>();
        using var rpc2 = _devicesManagedDeviceApprovalService.EscalateManagedDeviceAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.StreamProblemMetricsResponse
        {
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "StreamProblemMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiDevices.RejectManagedDeviceResponse> SendStreamProblemMetrics0Async(ApiDevices.RejectManagedDeviceRequest request, CancellationToken token)
    {
        return await _problemReportingEndpointSendStreamProblemMetrics0Port.SendAsync(request, token);
    }
}
