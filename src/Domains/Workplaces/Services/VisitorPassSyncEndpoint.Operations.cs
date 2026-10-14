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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class VisitorPassSyncEndpoint
{
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GetVisitorPassSyncStateResponse> GetVisitorPassSyncState(ApiWorkplaces.GetVisitorPassSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        _logger.LogInformation("Handling GetVisitorPassSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GetVisitorPassSyncState", "accepted", token);
        var input0 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient>("acme.employees.v1.EmploymentContractValidationService");
        using var rpc0 = named0.PreviewEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetVisitorPassSyncStateResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                },
                IssuedAt = (reply0.EmploymentContract?.IssuedAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists visitor passes a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListVisitorPassConflictsResponse> ListVisitorPassConflicts(ApiWorkplaces.ListVisitorPassConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListVisitorPassConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", "batch",
            "ListVisitorPassConflicts", "accepted", token);
        var response = new ApiWorkplaces.ListVisitorPassConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ListVisitorPassConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.PullVisitorPassesResponse> PullVisitorPasses(ApiWorkplaces.PullVisitorPassesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullVisitorPasses for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ValidateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (request.VisitorPass?.Audit?.Etag ?? ""),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.VisitorPass?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiWorkplaces.ValidateBuildingRequest, ApiWorkplaces.ValidateBuildingResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingValidationService", "ValidateBuilding",
            Wire.Marshaller<ApiWorkplaces.ValidateBuildingRequest>(), Wire.Marshaller<ApiWorkplaces.ValidateBuildingResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeSignatureUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Building?.Manager?.Email ?? ""),
                },
                Variables = { (request.VisitorPass?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsSignatureNotificationService.UnsubscribeSignatureUpdatesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.RetryDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Building?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    Iban = (request.VisitorPass?.Bank?.Iban ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.VisitorPass?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateExportService.RetryDocumentTemplateExportAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiWorkplaces.PullVisitorPassesResponse
        {
            Outcome = new ApiWorkplaces.PullVisitorPassesResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Signature?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Building?.Location?.Recipient ?? ""),
                },
                IssuedAt = (request.VisitorPass?.IssuedAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "PullVisitorPasses", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.PushVisitorPassResponse> PushVisitorPass(ApiWorkplaces.PushVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFraud.SuggestRiskAssessmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFraud.SuggestRiskAssessmentsRequest.Types.Filter
            {
                CreatedAfter = (request.VisitorPass?.DueAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _fraudRiskAssessmentQueryService.SuggestRiskAssessmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.PushVisitorPassResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                IssuedAt = (request.VisitorPass?.IssuedAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "PushVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ReconcileVisitorPassesResponse> ReconcileVisitorPasses(ApiWorkplaces.ReconcileVisitorPassesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileVisitorPasses for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.PreviewVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.VisitorPass?.Audit?.CreatedBy ?? ""),
                },
                Card = new ApiWorkplaces.VisitorPassCardSettlement
                {
                    Network = (request.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Card => (request.VisitorPass?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _workplacesVisitorPassValidationService.PreviewVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiInsurance.UpdatePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Policy = new ApiInsurance.Policy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiInsurance.PolicyCardFunding
                {
                    Last4 = (request.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Card => (request.VisitorPass?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _insurancePolicyService.UpdatePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ReconcileVisitorPassesResponse
        {
            Outcome = new ApiWorkplaces.ReconcileVisitorPassesResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.VisitorPass?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.Policy?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = (request.VisitorPass?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ReconcileVisitorPasses", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ResolveVisitorPassConflictResponse> ResolveVisitorPassConflict(ApiWorkplaces.ResolveVisitorPassConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveVisitorPassConflict for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CheckUserEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.CheckUserEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _identityUserValidationService.CheckUserEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ListChangeRequestReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListChangeRequestReportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.User ?? new ApiIdentity.User()).IdentityDocumentCase switch
                {
                    ApiIdentity.User.IdentityDocumentOneofCase.Passport => (reply0.User?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Region = (request.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _itsmChangeRequestReportingService.ListChangeRequestReportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ResolveVisitorPassConflictResponse
        {
            Outcome = new ApiWorkplaces.ResolveVisitorPassConflictResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.User?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Total = (request.VisitorPass?.Total ?? new ApiCommon.Money()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ResolveVisitorPassConflict", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of visitor passes with a peer.
    /// </summary>
    public override async Task StreamVisitorPassChanges(IAsyncStreamReader<ApiWorkplaces.StreamVisitorPassChangesRequest> requests, IServerStreamWriter<ApiWorkplaces.StreamVisitorPassChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamVisitorPassChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiLicensing.AdvanceSoftwareLicenseWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Bank = new ApiLicensing.SoftwareLicenseBankFunding
                    {
                        Iban = (request.Change?.Bank?.Iban ?? ""),
                    },
                },
            };
            using var rpc0 = _licensingSoftwareLicenseWorkflowService.AdvanceSoftwareLicenseWorkflowAsync(input0, cancellationToken: token);
            var pending0 = rpc0.ResponseAsync;
            var input1 = new ApiCorporateevents.DelegateRegistrationApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (request.Change?.Audit?.CreatedBy ?? ""),
                    },
                },
            };
            using var rpc1 = _corporateeventsRegistrationApprovalService.DelegateRegistrationApprovalAsync(input1, cancellationToken: token);
            var pending1 = rpc1.ResponseAsync;
            await Task.WhenAll(pending0, pending1);
            var reply0 = await pending0;
            var reply1 = await pending1;
            var response = new ApiWorkplaces.StreamVisitorPassChangesResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply1.Result?.Message ?? ""),
                },
                Sequence = (request.Ack?.Sequence ?? 0L),
                VisitorPass = new ApiWorkplaces.VisitorPass
                {
                    BillingAddress = (request.Change?.BillingAddress ?? new ApiCommon.PostalAddress()),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
                "audit", "StreamVisitorPassChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.SyncVisitorPassesResponse> SyncVisitorPasses(ApiWorkplaces.SyncVisitorPassesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncVisitorPasses for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.VisitorPass?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ScoreVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Bank = new ApiWorkplaces.VisitorPassBankSettlement
                {
                    AccountHolder = (request.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Bank => (request.VisitorPass?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (request.VisitorPass?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassValidationService.ScoreVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiBoxoffice.SummarizeBoxOfficeSalesResponse();
        foreach (var item in (request.VisitorPass?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.VisitorPassLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiBoxoffice.SummarizeBoxOfficeSalesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiBoxoffice.SummarizeBoxOfficeSalesRequest.Types.Details
                {
                    Filters = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            using var rpc2 = _boxofficeBoxOfficeSaleQueryService.SummarizeBoxOfficeSalesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    AccountHolder = (reply1.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Bank => (reply1.VisitorPass?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.SyncVisitorPassesResponse
        {
            Outcome = new ApiWorkplaces.SyncVisitorPassesResponse.Types.Outcome
            {
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.VisitorPass?.BillingAddress?.Region ?? ""),
                },
                Counterparty = (request.VisitorPass?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "SyncVisitorPasses", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
}
