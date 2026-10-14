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
using ApiConfiguration = Acme.Configuration.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ProblemLifecycleEndpoint
{
    /// <summary>
    /// Activates a problem.
    /// </summary>
    public override async Task<ApiItsm.ActivateProblemResponse> ActivateProblem(ApiItsm.ActivateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling ActivateProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "ActivateProblem", "accepted", token);
        var input0 = new ApiItsm.ExplainChangeRequestDecisionRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainChangeRequestDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient>("acme.itsm.v1.ChangeRequestValidationService");
        using var rpc0 = named0.ExplainChangeRequestDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizeEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ChangeRequest?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.ChangeRequest?.Requester?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.NormalizeEmploymentContractRequest, ApiEmployees.NormalizeEmploymentContractResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractValidationService", "NormalizeEmploymentContract",
            Wire.Marshaller<ApiEmployees.NormalizeEmploymentContractRequest>(), Wire.Marshaller<ApiEmployees.NormalizeEmploymentContractResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ValidateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ValidateProblemRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
                Sla = new ApiItsm.ProblemSla
                {
                    PolicyId = (reply0.ChangeRequest?.Sla?.PolicyId ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiItsm.ValidateProblemRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _itsmProblemValidationService.ValidateProblemAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiItsm.ActivateProblemRequest, ApiEmployees.SearchEmployeesRequest>(value => new ApiEmployees.SearchEmployeesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply0.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (value.Details?.NewOwner?.UserId ?? ""),
                Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _employeesEmployeeService.SearchEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ActivateProblemResponse
        {
            Outcome = new ApiItsm.ActivateProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                    PreferredChannel = (reply2.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ActivateProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a problem.
    /// </summary>
    public override async Task<ApiItsm.ExpireProblemResponse> ExpireProblem(ApiItsm.ExpireProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling ExpireProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "ExpireProblem", "accepted", token);
        var input0 = new ApiItsm.ShareServiceAssetReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareServiceAssetReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetReportingService.ShareServiceAssetReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.UnsubscribeDocumentTemplateUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (reply1.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply2 = await SendExpireProblem2Async(input2, token);
        var input3 = new ApiFeatureflags.ExportFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ExportFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.ServiceAsset?.Address?.Region ?? ""),
            },
        };
        using var stream3 = _featureflagsFeatureFlagQueryService.ExportFeatureFlags(input3, cancellationToken: token);
        await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportFeatureFlags frame {Type}", change3.GetType().Name);
        }
        var response = new ApiItsm.ExpireProblemResponse
        {
            Outcome = new ApiItsm.ExpireProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ExpireProblem", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.GetProblemLifecycleResponse> GetProblemLifecycle(ApiItsm.GetProblemLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling GetProblemLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GetProblemLifecycle", "accepted", token);
        var input0 = new ApiAudit.CreateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                CorrelationId = (request.ProblemId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventService.CreateAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Attributes = { (reply0.AuditEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                Number = (reply0.AuditEvent?.Subject?.Number ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.EstimateEmploymentContractRequest>(json1);
        using var rpc1 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.GetProblemLifecycleResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.EmploymentContract?.Audit?.Etag ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.AuditEvent?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a problem.
    /// </summary>
    public override async Task<ApiItsm.ReinstateProblemResponse> ReinstateProblem(ApiItsm.ReinstateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling ReinstateProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "ReinstateProblem", "accepted", token);
        var input0 = new ApiConfiguration.SearchConfigSetsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiConfiguration.SearchConfigSetsRequest.Types.Filter
        {
            CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
        };
        using var rpc0 = _configurationConfigSetService.SearchConfigSetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CompareServiceAssetOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmServiceAssetCalculationService.CompareServiceAssetOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ReinstateProblemResponse
        {
            Outcome = new ApiItsm.ReinstateProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Problem = new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply1.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ReinstateProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a problem.
    /// </summary>
    public override async Task<ApiItsm.RenewProblemResponse> RenewProblem(ApiItsm.RenewProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling RenewProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "RenewProblem", "accepted", token);
        var input0 = new ApiFeatureflags.BrowseFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.BrowseFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagQueryService.BrowseFeatureFlagsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.EstimateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.EstimateSignatureAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RenewProblemResponse
        {
            Outcome = new ApiItsm.RenewProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "RenewProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a problem.
    /// </summary>
    public override async Task<ApiItsm.ScheduleProblemExpiryResponse> ScheduleProblemExpiry(ApiItsm.ScheduleProblemExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleProblemExpiry for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.StartEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Problem?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Problem?.Requester?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingEntitlementWorkflowService.StartEntitlementWorkflowAsync);
        var reply1 = new ApiDocuments.LintSignaturesResponse();
        foreach (var item in (request.Problem?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDocuments.LintSignaturesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.LintSignaturesRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Problem?.Requester?.Phone ?? ""),
                        PreferredChannel = (request.Problem?.Requester?.PreferredChannel ?? ""),
                    },
                },
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDocuments.SignatureMeasurement
                    {
                        ObservedAt = (item.ChangedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc1 = _documentsSignatureValidationService.LintSignaturesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiAudit.BatchGetAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named2 = _registry.Get<ApiAudit.AuditEventService.AuditEventServiceClient>("acme.audit.v1.AuditEventService");
        using var rpc2 = named2.BatchGetAuditEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.ScheduleProblemExpiryResponse
        {
            Outcome = new ApiItsm.ScheduleProblemExpiryResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ScheduleProblemExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a problem.
    /// </summary>
    public override async Task<ApiItsm.SuspendProblemResponse> SuspendProblem(ApiItsm.SuspendProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling SuspendProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "SuspendProblem", "accepted", token);
        var input0 = new ApiDevices.EscalateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EscalateManagedDeviceRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiDevices.EscalateManagedDeviceRequest, ApiDevices.EscalateManagedDeviceResponse>(
            MethodType.Unary, "acme.devices.v1" + ".ManagedDeviceApprovalService", "EscalateManagedDevice",
            Wire.Marshaller<ApiDevices.EscalateManagedDeviceRequest>(), Wire.Marshaller<ApiDevices.EscalateManagedDeviceResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ShareChangeRequestReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareChangeRequestReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _itsmChangeRequestReportingService.ShareChangeRequestReportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.PullProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PullProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                    Email = (reply2.ChangeRequest?.Assignee?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc3 = _itsmProblemSyncService.PullProblemsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.SuspendProblemResponse
        {
            Outcome = new ApiItsm.SuspendProblemResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
                Subject = (reply2.ChangeRequest?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "SuspendProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a problem.
    /// </summary>
    public override async Task<ApiItsm.TerminateProblemResponse> TerminateProblem(ApiItsm.TerminateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling TerminateProblem for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "TerminateProblem", "accepted", token);
        var input0 = request.TerminateProblemToCancelDevicePolicyWorkflow0();
        using var rpc0 = _devicesDevicePolicyWorkflowService.CancelDevicePolicyWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ValidateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ValidateUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendTerminateProblem1Async(input1, token);
        var response = new ApiItsm.TerminateProblemResponse
        {
            Outcome = new ApiItsm.TerminateProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.User?.Contact?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "TerminateProblem", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.SummarizeOrgUnitsResponse> SendExpireProblem2Async(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token)
    {
        return await _problemLifecycleEndpointSendExpireProblem2Port.SendAsync(request, token);
    }
    private async Task<ApiIdentity.ValidateUserResponse> SendTerminateProblem1Async(ApiIdentity.ValidateUserRequest request, CancellationToken token)
    {
        return await _problemLifecycleEndpointSendTerminateProblem1Port.SendAsync(request, token);
    }
}
