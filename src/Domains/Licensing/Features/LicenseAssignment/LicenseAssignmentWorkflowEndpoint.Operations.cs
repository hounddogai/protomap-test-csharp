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

public sealed partial class LicenseAssignmentWorkflowEndpoint
{
    /// <summary>
    /// Advances a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.AdvanceLicenseAssignmentWorkflowResponse> AdvanceLicenseAssignmentWorkflow(ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ValidateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ValidateUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiIdentity.ValidateUserRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _identityUserValidationService.ValidateUserAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.AdvanceLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.User?.PrimaryAddress?.Recipient ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Total = (request.LicenseAssignment?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "AdvanceLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.CancelLicenseAssignmentWorkflowResponse> CancelLicenseAssignmentWorkflow(ApiLicensing.CancelLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainEscalationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainEscalationCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _helpdeskEscalationCalculationService.ExplainEscalationCalculationAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RenewLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RenewLicenseAssignmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
        };
        using var rpc1 = _licensingLicenseAssignmentLifecycleService.RenewLicenseAssignmentAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = request.CancelLicenseAssignmentWorkflowToSummarizeOnboardingPlans2(reply0);
        using var rpc2 = _onboardingOnboardingPlanQueryService.SummarizeOnboardingPlansAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.FetchLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
        };
        var reply3 = await SendCancelLicenseAssignmentWorkflow3Async(input3, token);
        var response = new ApiLicensing.CancelLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
                IssuedAt = (request.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.CancelLicenseAssignmentWorkflowResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LicenseAssignment?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "CancelLicenseAssignmentWorkflow", (request.LicenseAssignment?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.InspectLicenseAssignmentWorkflowResponse> InspectLicenseAssignmentWorkflow(ApiLicensing.InspectLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling InspectLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "InspectLicenseAssignmentWorkflow", "accepted", token);
        var response = new ApiLicensing.InspectLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists license assignments a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.ListLicenseAssignmentWorkflowsResponse> ListLicenseAssignmentWorkflows(ApiLicensing.ListLicenseAssignmentWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListLicenseAssignmentWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", "batch",
            "ListLicenseAssignmentWorkflows", "accepted", token);
        var response = new ApiLicensing.ListLicenseAssignmentWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ListLicenseAssignmentWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.PauseLicenseAssignmentWorkflowResponse> PauseLicenseAssignmentWorkflow(ApiLicensing.PauseLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.TerminateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.TerminateLicenseAssignmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingLicenseAssignmentLifecycleService.TerminateLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.PauseLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Notes = (request.LicenseAssignment?.Notes ?? ""),
                Status = (reply0.LicenseAssignment?.Status ?? (ApiLicensing.LicenseAssignmentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "PauseLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ResumeLicenseAssignmentWorkflowResponse> ResumeLicenseAssignmentWorkflow(ApiLicensing.ResumeLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    AccountHolder = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (request.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()).Select(async item =>
        {
            var input1 = new ApiLicensing.AdvanceEntitlementWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc1 = _licensingEntitlementWorkflowService.AdvanceEntitlementWorkflowAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiLicensing.AdvanceEntitlementWorkflowResponse();
        var response = new ApiLicensing.ResumeLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Status = (request.LicenseAssignment?.Status ?? (ApiLicensing.LicenseAssignmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ResumeLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.RetryLicenseAssignmentWorkflowResponse> RetryLicenseAssignmentWorkflow(ApiLicensing.RetryLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RecallSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseApprovalService.RecallSoftwareLicenseAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var batch2 = (request.LicenseAssignment?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentAttachment>()).Select(async item =>
        {
            var input2 = new ApiLicensing.LintLicenseAssignmentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LicenseAssignment = new ApiLicensing.LicenseAssignment
                {
                    Attachments = { new ApiLicensing.LicenseAssignmentAttachment
                    {
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc2 = _licensingLicenseAssignmentValidationService.LintLicenseAssignmentsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiLicensing.LintLicenseAssignmentsResponse();
        var input3 = new ApiDocuments.StreamDocumentTemplateChangesRequest
        {
            Change = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                },
                Card = new ApiDocuments.DocumentTemplateCardSettlement
                {
                    HolderName = (reply2.LicenseAssignment?.Card?.HolderName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var stream3 = _documentsDocumentTemplateSyncService.StreamDocumentTemplateChanges(cancellationToken: token);
        async Task Drain3Async()
        {
            await foreach (var change in stream3.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received peer frame {Type}", change.GetType().Name);
            }
        }
        var reader3 = Drain3Async();
        await stream3.RequestStream.WriteAsync(input3, token);
        await stream3.RequestStream.CompleteAsync();
        await reader3;
        var response = new ApiLicensing.RetryLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Number = (request.LicenseAssignment?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "RetryLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.SignalLicenseAssignmentWorkflowResponse> SignalLicenseAssignmentWorkflow(ApiLicensing.SignalLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RenewEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RenewEntitlementRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            EntitlementId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingEntitlementLifecycleService.RenewEntitlementAsync);
        var input1 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Entitlement?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _sendSignalLicenseAssignmentWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.SignalLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                DueAt = (request.LicenseAssignment?.DueAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.SignalLicenseAssignmentWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "SignalLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.StartLicenseAssignmentWorkflowResponse> StartLicenseAssignmentWorkflow(ApiLicensing.StartLicenseAssignmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartLicenseAssignmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient>("acme.documents.v1.DocumentTemplateNotificationService");
        using var rpc0 = named0.ListDocumentTemplateSubscriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RecallLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Bank = new ApiLicensing.LicenseAssignmentBankSettlement
                {
                    Iban = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (request.LicenseAssignment?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentApprovalService.RecallLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.StartLicenseAssignmentWorkflowResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                DueAt = (request.LicenseAssignment?.DueAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "StartLicenseAssignmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.FetchLicenseAssignmentReportResponse> SendCancelLicenseAssignmentWorkflow3Async(ApiLicensing.FetchLicenseAssignmentReportRequest request, CancellationToken token)
    {
        return await _licenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port.SendAsync(request, token);
    }
}
