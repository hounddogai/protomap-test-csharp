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
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingPlanWorkflowEndpoint
{
    /// <summary>
    /// Advances an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.AdvanceOnboardingPlanWorkflowResponse> AdvanceOnboardingPlanWorkflow(ApiOnboarding.AdvanceOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDocuments.ExplainDocumentTemplateCalculationResponse();
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()))
        {
            var input0 = new ApiDocuments.ExplainDocumentTemplateCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Lines = { new ApiDocuments.DocumentTemplateLine
                    {
                        Tax = new ApiDocuments.DocumentTemplateLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc0 = _documentsDocumentTemplateCalculationService.ExplainDocumentTemplateCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiFeatureflags.ExportFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ExportFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _featureflagsFeatureFlagQueryService.ExportFeatureFlags(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportFeatureFlags frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentTemplate?.IssuedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiFeatureflags.CountFeatureFlagsRequest>(json2);
        using var rpc2 = _featureflagsFeatureFlagQueryService.CountFeatureFlagsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.AdvanceOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Notes = (request.OnboardingPlan?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "AdvanceOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CancelOnboardingPlanWorkflowResponse> CancelOnboardingPlanWorkflow(ApiOnboarding.CancelOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.SignalSoftwareLicenseWorkflowResponse();
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()))
        {
            var input0 = new ApiLicensing.SignalSoftwareLicenseWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Card = new ApiLicensing.SoftwareLicenseCardFunding
                    {
                        Token = (request.OnboardingPlan?.Card?.Token ?? ""),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                    },
                    Holder = new ApiLicensing.LicenseAssignmentRef
                    {
                        DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                    },
                    Limits = { new ApiLicensing.SoftwareLicenseLimit
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc0 = _licensingSoftwareLicenseWorkflowService.SignalSoftwareLicenseWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiHelpdesk.SignalSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.SignalSlaPolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (reply0.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskSlaPolicyWorkflowService.SignalSlaPolicyWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CancelOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
            Outcome = new ApiOnboarding.CancelOnboardingPlanWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CancelOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.InspectOnboardingPlanWorkflowResponse> InspectOnboardingPlanWorkflow(ApiOnboarding.InspectOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling InspectOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "InspectOnboardingPlanWorkflow", "accepted", token);
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OnboardingPlanId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiHelpdesk.RetrySlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RetrySlaPolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyWorkflowService.RetrySlaPolicyWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.InspectOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply1.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListOnboardingPlanWorkflowsResponse> ListOnboardingPlanWorkflows(ApiOnboarding.ListOnboardingPlanWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListOnboardingPlanWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "ListOnboardingPlanWorkflows", "accepted", token);
        var response = new ApiOnboarding.ListOnboardingPlanWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ListOnboardingPlanWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.PauseOnboardingPlanWorkflowResponse> PauseOnboardingPlanWorkflow(ApiOnboarding.PauseOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ExpireEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExpireEquipmentRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            EquipmentId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _onboardingEquipmentLifecycleService.ExpireEquipmentAsync);
        var input1 = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Equipment?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _sendPauseOnboardingPlanWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.CommentEnrollmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Enrollment = new ApiBenefits.Enrollment
            {
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient>("acme.benefits.v1.EnrollmentApprovalService");
        using var rpc2 = named2.CommentEnrollmentApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.PauseOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Equipment?.Address?.CountryCode ?? ""),
                },
                Notes = (request.OnboardingPlan?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.PauseOnboardingPlanWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "PauseOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ResumeOnboardingPlanWorkflowResponse> ResumeOnboardingPlanWorkflow(ApiOnboarding.ResumeOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.StartSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Bic = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseWorkflowService.StartSoftwareLicenseWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CommentEscalationApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiHelpdesk.CommentEscalationApprovalRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _helpdeskEscalationApprovalService.CommentEscalationApprovalAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiOnboarding.ResumeOnboardingPlanWorkflowRequest, ApiSearch.FindSearchIndexesRequest>(value => new ApiSearch.FindSearchIndexesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (value.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _searchSearchIndexQueryService.FindSearchIndexesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.SimulateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Escalation?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _onboardingOnboardingTaskCalculationService.SimulateOnboardingTaskAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.ResumeOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Escalation?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply3.OnboardingTask?.Audit?.UpdatedBy ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Total = (request.OnboardingPlan?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiOnboarding.ResumeOnboardingPlanWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ResumeOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.RetryOnboardingPlanWorkflowResponse> RetryOnboardingPlanWorkflow(ApiOnboarding.RetryOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRouting.CompareRouteOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRouting.CompareRouteOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Route = new ApiRouting.Route
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiRouting.RouteBankSettlement
                {
                    Iban = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _routingRouteCalculationService.CompareRouteOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.SummarizeVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await SendRetryOnboardingPlanWorkflow1Async(input1, token);
        var input2 = new ApiLineage.ResolveLineageNodesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _lineageLineageNodeQueryService.ResolveLineageNodesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.RetryOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                IssuedAt = (request.OnboardingPlan?.IssuedAt ?? new Timestamp()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.RetryOnboardingPlanWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingPlan?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "RetryOnboardingPlanWorkflow", (request.OnboardingPlan?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.SignalOnboardingPlanWorkflowResponse> SignalOnboardingPlanWorkflow(ApiOnboarding.SignalOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EscalateEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                Customer = new ApiHelpdesk.TicketRef
                {
                    DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.EscalateEscalationRequest>();
        using var rpc0 = _helpdeskEscalationApprovalService.EscalateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Escalation?.Requester?.PreferredChannel ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLicensing.PauseSoftwareLicenseWorkflowRequest>(json1);
        using var rpc1 = _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.SignalOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SignalOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.StartOnboardingPlanWorkflowResponse> StartOnboardingPlanWorkflow(ApiOnboarding.StartOnboardingPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartOnboardingPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiOnboarding.ApproveOnboardingPlanResponse();
        foreach (var item in (request.OnboardingPlan?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanAttachment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiOnboarding.ApproveOnboardingPlanRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                OnboardingPlan = new ApiOnboarding.OnboardingPlan
                {
                    Attachments = { new ApiOnboarding.OnboardingPlanAttachment
                    {
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc0 = _onboardingOnboardingPlanApprovalService.ApproveOnboardingPlanAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiOnboarding.ExpireEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExpireEquipmentRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            EquipmentId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
        };
        using var rpc1 = _onboardingEquipmentLifecycleService.ExpireEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.StartOnboardingPlanWorkflowResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "StartOnboardingPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiVenues.SummarizeVenuesResponse> SendRetryOnboardingPlanWorkflow1Async(ApiVenues.SummarizeVenuesRequest request, CancellationToken token)
    {
        return await _onboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port.SendAsync(request, token);
    }
}
