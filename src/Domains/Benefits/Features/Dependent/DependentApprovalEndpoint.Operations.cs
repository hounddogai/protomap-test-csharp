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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class DependentApprovalEndpoint
{
    /// <summary>
    /// Approves a dependent.
    /// </summary>
    public override async Task<ApiBenefits.ApproveDependentResponse> ApproveDependent(ApiBenefits.ApproveDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EscalateEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EscalateEscalationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.EscalateEscalationRequest>();
        using var rpc0 = _helpdeskEscalationApprovalService.EscalateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Dependent?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.StartBenefitPlanWorkflowRequest();
        input2.BenefitPlan = new ApiBenefits.BenefitPlan
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.Dependent?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc2 = _benefitsBenefitPlanWorkflowService.StartBenefitPlanWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.ApproveDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Escalation?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
            },
            Outcome = new ApiBenefits.ApproveDependentResponse.Types.Outcome
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ApproveDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a dependent.
    /// </summary>
    public override async Task<ApiBenefits.DelegateDependentApprovalResponse> DelegateDependentApproval(ApiBenefits.DelegateDependentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateDependentApproval for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.PauseServiceAssetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PauseServiceAssetWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Dependent?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Dependent?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetWorkflowService.PauseServiceAssetWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ListComplianceScanWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListComplianceScanWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent ?? new ApiBenefits.Dependent()).IdentityDocumentCase switch
                {
                    ApiBenefits.Dependent.IdentityDocumentOneofCase.Passport => (request.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                OwnerId = (reply0.ServiceAsset?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _devicesComplianceScanWorkflowService.ListComplianceScanWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.CalculateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.CalculateLifeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Dependent?.PrimaryAddress?.CountryCode ?? ""),
                    Lines = { (request.Dependent?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Description = (reply0.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc3 = _recruitingJobOpeningCalculationService.EstimateJobOpeningAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.DelegateDependentApprovalResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.JobOpening?.Audit?.Etag ?? ""),
                },
                Contact = (request.Dependent?.Contact ?? new ApiCommon.ContactInfo()),
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply2.LifeEvent?.Subject?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.DelegateDependentApprovalResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "DelegateDependentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a dependent.
    /// </summary>
    public override async Task<ApiBenefits.EscalateDependentResponse> EscalateDependent(ApiBenefits.EscalateDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CheckLifeEventEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventValidationService.CheckLifeEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.EscalateDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
                Status = (request.Dependent?.Status ?? (ApiBenefits.DependentStatus)0),
            },
            Outcome = new ApiBenefits.EscalateDependentResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "EscalateDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists dependents a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListPendingDependentsResponse> ListPendingDependents(ApiBenefits.ListPendingDependentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingDependents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "dependent", "batch",
            "ListPendingDependents", "accepted", token);
        var input0 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ListPendingDependentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ListPendingDependents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a dependent.
    /// </summary>
    public override async Task<ApiBenefits.RecallDependentResponse> RecallDependent(ApiBenefits.RecallDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.RecalculateLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RecalculateLifeEventsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Dependent?.PrimaryAddress?.Locality ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.RecalculateLifeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.RetryBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Dependent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RetryBenefitPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiBenefits.RetryBenefitPlanWorkflowRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _benefitsBenefitPlanWorkflowService.RetryBenefitPlanWorkflowAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.RecallDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
            },
            Outcome = new ApiBenefits.RecallDependentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "RecallDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a dependent.
    /// </summary>
    public override async Task<ApiBenefits.RejectDependentResponse> RejectDependent(ApiBenefits.RejectDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiBenefits.RejectDependentRequest, ApiHelpdesk.EscalateEscalationRequest>(value => new ApiHelpdesk.EscalateEscalationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (value.Details?.Approver?.DisplayName ?? ""),
                    Email = (value.Dependent?.RelationshipManager?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.Dependent?.Audit?.Etag ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskEscalationApprovalService.EscalateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.RejectDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
                Status = (request.Dependent?.Status ?? (ApiBenefits.DependentStatus)0),
            },
            Outcome = new ApiBenefits.RejectDependentResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "RejectDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a dependent.
    /// </summary>
    public override async Task<ApiBenefits.SubmitDependentResponse> SubmitDependent(ApiBenefits.SubmitDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.ResolveEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _benefitsEnrollmentQueryService.ResolveEnrollmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SubmitDependentToResumeBenefitPlanWorkflow1();
        using var rpc1 = _benefitsBenefitPlanWorkflowService.ResumeBenefitPlanWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.SubmitDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
            },
            Outcome = new ApiBenefits.SubmitDependentResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "SubmitDependent", (request.Reason ?? ""), token);
        return response;
    }
}
