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
using ApiDevices = Acme.Devices.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ComplianceScanApprovalEndpoint
{
    /// <summary>
    /// Approves a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ApproveComplianceScanResponse> ApproveComplianceScan(ApiDevices.ApproveComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScan?.ComplianceScanId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Sequence = (request.ComplianceScan?.Sequence ?? 0L),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.AdvanceManagedDeviceWorkflowRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiDevices.AdvanceManagedDeviceWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.ComplianceScan?.Subject?.Email ?? ""),
            },
        };
        input1.ManagedDevice = new ApiDevices.ManagedDevice
        {
            Owner = new ApiIdentity.UserRef
            {
                UserId = (request.Details?.Approver?.UserId ?? ""),
            },
        };
        using var rpc1 = _devicesManagedDeviceWorkflowService.AdvanceManagedDeviceWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDevices.AdvanceManagedDeviceWorkflowRequest>("acme.devices.v1.ComplianceScanApprovalService/ApproveComplianceScan/1", input1.Clone()), token);
        var input2 = new ApiDevices.TerminateComplianceScanRequest
        {
            ComplianceScanId = (request.ComplianceScan?.ComplianceScanId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.TerminateComplianceScanRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanLifecycleService.TerminateComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.ApproveComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply2.ComplianceScan?.CorrelationId ?? ""),
                OccurredAt = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
                Subject = (reply2.ComplianceScan?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiDevices.ApproveComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ApproveComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.CommentComplianceScanApprovalResponse> CommentComplianceScanApproval(ApiDevices.CommentComplianceScanApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentComplianceScanApproval for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.ComplianceScan?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDevices.ComplianceScanMeasurement>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.EstimateOfferRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                    IssuedAt = (item.ObservedAt ?? new Timestamp()),
                    Lines = { new ApiRecruiting.OfferLine
                    {
                        Attributes = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    } },
                },
            };
            using var rpc0 = _recruitingOfferCalculationService.EstimateOfferAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.EstimateOfferResponse();
        var input1 = new ApiDrivers.CreateDriverRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Driver = new ApiDrivers.Driver
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Offer?.BillingAddress?.Locality ?? ""),
                    Region = (reply0.Offer?.BillingAddress?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _driversDriverService.CreateDriverAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.CommentComplianceScanApprovalResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "CommentComplianceScanApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.DelegateComplianceScanApprovalResponse> DelegateComplianceScanApproval(ApiDevices.DelegateComplianceScanApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateComplianceScanApproval for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ResolveAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _authorizationAccessPolicyQueryService.ResolveAccessPoliciesAsync);
        var response = new ApiDevices.DelegateComplianceScanApprovalResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Outcome = new ApiDevices.DelegateComplianceScanApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "DelegateComplianceScanApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.EscalateComplianceScanResponse> EscalateComplianceScan(ApiDevices.EscalateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SubmitDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SubmitDevicePolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                ApproverEmail = (request.Details?.ApproverEmail ?? ""),
            },
        };
        using var rpc0 = _sendEscalateComplianceScan0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainInterviewDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                    UserId = (reply0.DevicePolicy?.Owner?.UserId ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                } },
            },
        };
        var named1 = _registry.Get<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService");
        using var rpc1 = named1.ExplainInterviewDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.EscalateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Interview?.Organizer?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.EscalateComplianceScanResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "EscalateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.GetComplianceScanApprovalStatusResponse> GetComplianceScanApprovalStatus(ApiDevices.GetComplianceScanApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling GetComplianceScanApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "GetComplianceScanApprovalStatus", "accepted", token);
        var input0 = new ApiWorkplaces.ResolveDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiWorkplaces.ResolveDesksRequest, ApiWorkplaces.ResolveDesksResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".DeskQueryService", "ResolveDesks",
            Wire.Marshaller<ApiWorkplaces.ResolveDesksRequest>(), Wire.Marshaller<ApiWorkplaces.ResolveDesksResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetComplianceScanApprovalStatusResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Recalls a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.RecallComplianceScanResponse> RecallComplianceScan(ApiDevices.RecallComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var command0 = new DispatchCommand<ApiRecruiting.CheckCandidateEligibilityRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.RecallComplianceScanRequest, ApiRecruiting.ExplainCandidateCalculationRequest>(value => new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Preferences = new ApiRecruiting.CandidatePreferences
                {
                    Language = (reply0.Candidate?.Preferences?.Language ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Candidate?.PrimaryAddress?.Locality ?? ""),
                    Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.AdvanceManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Candidate?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Candidate?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Candidate?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _devicesManagedDeviceWorkflowService.AdvanceManagedDeviceWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.QuoteOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteOfferRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply2.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply2.ManagedDevice?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _recruitingOfferCalculationService.QuoteOfferAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.RecallComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply2.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.RecallComplianceScanResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "RecallComplianceScan", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.SubmitComplianceScanResponse> SubmitComplianceScan(ApiDevices.SubmitComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.ComplianceScan?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDevices.ComplianceScanMeasurement>()))
        {
            var input0 = new ApiDrivers.WatchDriversRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDrivers.WatchDriversRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                },
                Since = (request.Details?.EffectiveAt ?? new Timestamp()),
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            };
            using var stream0 = _driversDriverService.WatchDrivers(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received WatchDrivers frame {Type}", change0.GetType().Name);
            }
        }
        var input1 = new ApiRecruiting.CalculateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingOfferCalculationService.CalculateOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.BatchGetManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiDevices.BatchGetManagedDevicesRequest>();
        using var rpc2 = _devicesManagedDeviceService.BatchGetManagedDevicesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.SubmitComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                OccurredAt = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "SubmitComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
}
