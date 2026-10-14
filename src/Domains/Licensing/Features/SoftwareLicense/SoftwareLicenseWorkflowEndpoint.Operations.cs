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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class SoftwareLicenseWorkflowEndpoint
{
    /// <summary>
    /// Advances a software license.
    /// </summary>
    public override async Task<ApiLicensing.AdvanceSoftwareLicenseWorkflowResponse> AdvanceSoftwareLicenseWorkflow(ApiLicensing.AdvanceSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CreateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseService.CreateSoftwareLicenseAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiTelematics.SummarizeTelemetryReadingsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTelematics.SummarizeTelemetryReadingsRequest.Types.Details
            {
                Filters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _telematicsTelemetryReadingQueryService.SummarizeTelemetryReadingsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var reply2 = new ApiLicensing.CompareEntitlementOptionsResponse();
        foreach (var item in (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiLicensing.CompareEntitlementOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Name = (item.Name ?? ""),
                },
            };
            using var rpc2 = _licensingEntitlementCalculationService.CompareEntitlementOptionsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiDocuments.ListDocumentSubscriptionsRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Filter = new ApiDocuments.ListDocumentSubscriptionsRequest.Types.Filter
        {
            CreatedAfter = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
            Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc3 = _documentsDocumentNotificationService.ListDocumentSubscriptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.AdvanceSoftwareLicenseWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Available = (request.SoftwareLicense?.Available ?? new ApiCommon.Money()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "AdvanceSoftwareLicenseWorkflow", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a software license.
    /// </summary>
    public override async Task<ApiLicensing.CancelSoftwareLicenseWorkflowResponse> CancelSoftwareLicenseWorkflow(ApiLicensing.CancelSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ExplainSoftwareLicenseCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Token = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    LicenseAssignmentId = (request.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.ExplainSoftwareLicenseCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RejectSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Token = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SoftwareLicense?.Contact?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                    LicenseAssignmentId = (request.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseApprovalService.RejectSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CancelSoftwareLicenseWorkflowResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = (request.SoftwareLicense?.Contact ?? new ApiCommon.ContactInfo()),
                Holder = (reply0.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CancelSoftwareLicenseWorkflow", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.InspectSoftwareLicenseWorkflowResponse> InspectSoftwareLicenseWorkflow(ApiLicensing.InspectSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        _logger.LogInformation("Handling InspectSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "InspectSoftwareLicenseWorkflow", "accepted", token);
        var input0 = new ApiLicensing.ScheduleEntitlementExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementLifecycleService.ScheduleEntitlementExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.InspectSoftwareLicenseWorkflowResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Pauses a software license.
    /// </summary>
    public override async Task<ApiLicensing.PauseSoftwareLicenseWorkflowResponse> PauseSoftwareLicenseWorkflow(ApiLicensing.PauseSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ExplainGoalDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _performanceGoalValidationService.ExplainGoalDecisionAsync);
        var reply1 = new ApiLicensing.EscalateEntitlementResponse();
        foreach (var item in (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiLicensing.EscalateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                    },
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Name = (item.Name ?? ""),
                },
            };
            using var rpc1 = _licensingEntitlementApprovalService.EscalateEntitlementAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLicensing.PauseSoftwareLicenseWorkflowResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = (request.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "PauseSoftwareLicenseWorkflow", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a software license.
    /// </summary>
    public override async Task<ApiLicensing.RetrySoftwareLicenseWorkflowResponse> RetrySoftwareLicenseWorkflow(ApiLicensing.RetrySoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetrySoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(async item =>
        {
            var input0 = new ApiDocuments.PreviewDocumentFolderRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentFolder = new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                        },
                        Children = { new ApiDocuments.DocumentFolder
                        {
                            Audit = new ApiCommon.AuditInfo
                            {
                                CreatedAt = (request.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                            },
                            Children = { new ApiDocuments.DocumentFolder
                            {
                                Name = (item.Name ?? ""),
                            } },
                        } },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.SoftwareLicense?.Holder?.DisplayName ?? ""),
                        },
                    } },
                },
            };
            using var rpc0 = _documentsDocumentFolderValidationService.PreviewDocumentFolderAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.PreviewDocumentFolderResponse();
        var input1 = new ApiLicensing.FindSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.FindSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var descriptor1 = new Method<ApiLicensing.FindSoftwareLicensesRequest, ApiLicensing.FindSoftwareLicensesResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseQueryService", "FindSoftwareLicenses",
            Wire.Marshaller<ApiLicensing.FindSoftwareLicensesRequest>(), Wire.Marshaller<ApiLicensing.FindSoftwareLicensesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.RetrySoftwareLicenseWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Holder = (request.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "RetrySoftwareLicenseWorkflow", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a software license.
    /// </summary>
    public override async Task<ApiLicensing.SignalSoftwareLicenseWorkflowResponse> SignalSoftwareLicenseWorkflow(ApiLicensing.SignalSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CloneSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SoftwareLicense?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    AccountHolder = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (request.SoftwareLicense?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (request.SoftwareLicense?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseService.CloneSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.ExplainFeedbackDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackValidationService.ExplainFeedbackDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.SignalSoftwareLicenseWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SoftwareLicenseId = (reply0.SoftwareLicense?.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SignalSoftwareLicenseWorkflow", (request.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a software license.
    /// </summary>
    public override async Task<ApiLicensing.StartSoftwareLicenseWorkflowResponse> StartSoftwareLicenseWorkflow(ApiLicensing.StartSoftwareLicenseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartSoftwareLicenseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidateEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.StartSoftwareLicenseWorkflowToNormalizeDocumentFolder1(reply0);
        using var rpc1 = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.StartSoftwareLicenseWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                OpenedAt = (request.SoftwareLicense?.OpenedAt ?? new Timestamp()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "StartSoftwareLicenseWorkflow", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
}
