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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ServiceAssetWorkflowEndpoint
{
    /// <summary>
    /// Advances a service asset.
    /// </summary>
    public override async Task<ApiItsm.AdvanceServiceAssetWorkflowResponse> AdvanceServiceAssetWorkflow(ApiItsm.AdvanceServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.GenerateProblemReportRequest();
        input0.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Reason = (request.Reason ?? "");
        using var rpc0 = _itsmProblemReportingService.GenerateProblemReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.AdvanceServiceAssetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Name = (request.ServiceAsset?.Name ?? ""),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "AdvanceServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a service asset.
    /// </summary>
    public override async Task<ApiItsm.CancelServiceAssetWorkflowResponse> CancelServiceAssetWorkflow(ApiItsm.CancelServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SignalEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementWorkflowService.SignalEntitlementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.BatchGetAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _auditAuditEventService.BatchGetAuditEventsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.CancelServiceAssetWorkflowResponse
        {
            Outcome = new ApiItsm.CancelServiceAssetWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = (request.ServiceAsset?.Owner ?? new ApiIdentity.UserRef()),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CancelServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a service asset.
    /// </summary>
    public override async Task<ApiItsm.InspectServiceAssetWorkflowResponse> InspectServiceAssetWorkflow(ApiItsm.InspectServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling InspectServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "InspectServiceAssetWorkflow", "accepted", token);
        var input0 = new ApiItsm.TerminateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.TerminateServiceAssetRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            ServiceAssetId = (request.ServiceAssetId ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmServiceAssetLifecycleService.TerminateServiceAssetAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.InspectServiceAssetWorkflowResponse
        {
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Name = (reply0.ServiceAsset?.Name ?? ""),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists service assets a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListServiceAssetWorkflowsResponse> ListServiceAssetWorkflows(ApiItsm.ListServiceAssetWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListServiceAssetWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", "batch",
            "ListServiceAssetWorkflows", "accepted", token);
        var response = new ApiItsm.ListServiceAssetWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ListServiceAssetWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a service asset.
    /// </summary>
    public override async Task<ApiItsm.PauseServiceAssetWorkflowResponse> PauseServiceAssetWorkflow(ApiItsm.PauseServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.SuggestLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SuggestLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.ServiceAsset?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _ledgerLedgerAccountQueryService.SuggestLedgerAccountsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendPauseServiceAssetWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.PauseServiceAssetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "PauseServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a service asset.
    /// </summary>
    public override async Task<ApiItsm.ResumeServiceAssetWorkflowResponse> ResumeServiceAssetWorkflow(ApiItsm.ResumeServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ServiceAssetId ?? ""),
                Region = (request.ServiceAsset?.Address?.Region ?? ""),
            },
        };
        using var stream0 = _employeesOrgUnitQueryService.ExportOrgUnits(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.RecalculateIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.RecalculateIncidentsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            Incident = new ApiItsm.Incident
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _itsmIncidentCalculationService.RecalculateIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply1.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Incident?.Assignee?.UserId ?? ""),
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.ResumeServiceAssetWorkflowResponse
        {
            Outcome = new ApiItsm.ResumeServiceAssetWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ResumeServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a service asset.
    /// </summary>
    public override async Task<ApiItsm.RetryServiceAssetWorkflowResponse> RetryServiceAssetWorkflow(ApiItsm.RetryServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiItsm.RetryServiceAssetWorkflowRequest, ApiItsm.PauseChangeRequestWorkflowRequest>(value => new ApiItsm.PauseChangeRequestWorkflowRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmChangeRequestWorkflowService.PauseChangeRequestWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeesPositionValidationService.ScorePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RetryServiceAssetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Name = (request.ServiceAsset?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ChangeRequest?.Customer?.DisplayName ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "RetryServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a service asset.
    /// </summary>
    public override async Task<ApiItsm.SignalServiceAssetWorkflowResponse> SignalServiceAssetWorkflow(ApiItsm.SignalServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PauseComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PauseComplianceScanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanWorkflowService.PauseComplianceScanWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.SuggestVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SuggestVenuesRequest.Types.Filter
            {
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _venuesVenueQueryService.SuggestVenuesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.SignalServiceAssetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "SignalServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a service asset.
    /// </summary>
    public override async Task<ApiItsm.StartServiceAssetWorkflowResponse> StartServiceAssetWorkflow(ApiItsm.StartServiceAssetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartServiceAssetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CalculateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ServiceAsset?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CalculateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc0 = _itsmChangeRequestCalculationService.CalculateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.SuggestVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SuggestVenuesRequest.Types.Filter
            {
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _venuesVenueQueryService.SuggestVenuesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.StartServiceAssetWorkflowResponse
        {
            Outcome = new ApiItsm.StartServiceAssetWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = (request.ServiceAsset?.Owner ?? new ApiIdentity.UserRef()),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "StartServiceAssetWorkflow", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
}
