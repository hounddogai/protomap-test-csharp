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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelehealth = Acme.Telehealth.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ChangeRequestValidationEndpoint
{
    /// <summary>
    /// Checks a change request.
    /// </summary>
    public override async Task<ApiItsm.CheckChangeRequestEligibilityResponse> CheckChangeRequestEligibility(ApiItsm.CheckChangeRequestEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckChangeRequestEligibility for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SimulateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Channel = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Channel => (request.ChangeRequest?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _itsmIncidentCalculationService.SimulateIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckIncidentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _itsmIncidentValidationService.CheckIncidentEligibilityAsync);
        var reply2 = new ApiDocuments.ListDocumentConflictsResponse();
        foreach (var item in (request.ChangeRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiDocuments.ListDocumentConflictsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    OwnerId = (reply0.Incident?.Sla?.PolicyId ?? ""),
                },
            };
            using var rpc2 = _documentsDocumentSyncService.ListDocumentConflictsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiBoxoffice.CloneBoxOfficeSaleRequest
        {
            BoxOfficeSale = new ApiBoxoffice.BoxOfficeSale
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Incident?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.Incident?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named3 = _registry.Get<ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient>("acme.boxoffice.v1.BoxOfficeSaleService");
        using var rpc3 = named3.CloneBoxOfficeSaleAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.CheckChangeRequestEligibilityResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Incident?.Requester?.Email ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (request.ChangeRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Incident?.Customer?.Email ?? ""),
                },
                Sla = new ApiItsm.ChangeRequestSla
                {
                    PausedFor = (reply0.Incident?.Sla?.PausedFor ?? new Duration()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CheckChangeRequestEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a change request.
    /// </summary>
    public override async Task<ApiItsm.ExplainChangeRequestDecisionResponse> ExplainChangeRequestDecision(ApiItsm.ExplainChangeRequestDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainChangeRequestDecision for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.QuoteDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiDevices.QuoteDevicePolicyRequest, ApiDevices.QuoteDevicePolicyResponse>(
            MethodType.Unary, "acme.devices.v1" + ".DevicePolicyCalculationService", "QuoteDevicePolicy",
            Wire.Marshaller<ApiDevices.QuoteDevicePolicyRequest>(), Wire.Marshaller<ApiDevices.QuoteDevicePolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
                Limits = { new ApiLicensing.SoftwareLicenseLimit
                {
                    Amount = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Discount => (reply0.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseValidationService.ScoreSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.ChangeRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestStatusChange>()).Select(async item =>
        {
            var input2 = new ApiDevices.FindDevicePoliciesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDevices.FindDevicePoliciesRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Labels = { (reply1.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (reply1.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            };
            using var rpc2 = _devicesDevicePolicyQueryService.FindDevicePoliciesAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiDevices.FindDevicePoliciesResponse();
        var input3 = new ApiItsm.RecalculateServiceAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply1.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _itsmServiceAssetCalculationService.RecalculateServiceAssetsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ExplainChangeRequestDecisionResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.ServiceAsset?.Audit?.Etag ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Priority = (reply0.DevicePolicy?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply1.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Subject = (request.ChangeRequest?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ExplainChangeRequestDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.GetChangeRequestRulesResponse> GetChangeRequestRules(ApiItsm.GetChangeRequestRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling GetChangeRequestRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "GetChangeRequestRules", "accepted", token);
        var input0 = request.GetChangeRequestRulesToValidateIncident0();
        using var rpc0 = _itsmIncidentValidationService.ValidateIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetChangeRequestRulesResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a change request.
    /// </summary>
    public override async Task<ApiItsm.LintChangeRequestsResponse> LintChangeRequests(ApiItsm.LintChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintChangeRequests for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()).Select(async item =>
        {
            var input0 = new ApiTelehealth.CalculateVideoVisitRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                VideoVisit = new ApiTelehealth.VideoVisit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Organizer = new ApiIdentity.UserRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                    Participants = { new ApiTelehealth.VideoVisitParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Name = (request.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                        },
                    } },
                },
            };
            using var rpc0 = _telehealthVideoVisitCalculationService.CalculateVideoVisitAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiTelehealth.CalculateVideoVisitResponse();
        var input1 = new ApiDocuments.UnsubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details
            {
                Channel = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                {
                    ApiItsm.ChangeRequest.OriginOneofCase.Channel => (request.ChangeRequest?.Channel?.Channel ?? ""),
                    _ => ""
                },
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureNotificationService.UnsubscribeSignatureUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.LintChangeRequestsResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Subject = (request.ChangeRequest?.Subject ?? ""),
            },
            Outcome = new ApiItsm.LintChangeRequestsResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "LintChangeRequests", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a change request.
    /// </summary>
    public override async Task<ApiItsm.NormalizeChangeRequestResponse> NormalizeChangeRequest(ApiItsm.NormalizeChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.RecalculateServiceAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.ChangeRequest?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetCalculationService.RecalculateServiceAssetsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckIncidentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmIncidentValidationService.CheckIncidentEligibilityAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiItsm.NormalizeChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (reply1.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "NormalizeChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a change request.
    /// </summary>
    public override async Task<ApiItsm.PreviewChangeRequestResponse> PreviewChangeRequest(ApiItsm.PreviewChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RecordDocumentVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordDocumentVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.RecordDocumentVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.RecalculateDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateDevicePoliciesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Document?.BillingAddress?.CountryCode ?? ""),
                    Region = (reply0.Document?.BillingAddress?.Region ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ChangeRequest?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyCalculationService.RecalculateDevicePoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.PreviewChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
            Outcome = new ApiItsm.PreviewChangeRequestResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "PreviewChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a change request.
    /// </summary>
    public override async Task<ApiItsm.ScoreChangeRequestResponse> ScoreChangeRequest(ApiItsm.ScoreChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentValidationService.ValidateLicenseAssignmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.EstimateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmServiceAssetCalculationService.EstimateServiceAssetAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.ScoreChangeRequestResponse
        {
            Breakdown = new ApiItsm.ScoreChangeRequestResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
                Total = (reply0.LicenseAssignment?.Total ?? new ApiCommon.Money()),
            },
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ScoreChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a change request.
    /// </summary>
    public override async Task<ApiItsm.ValidateChangeRequestResponse> ValidateChangeRequest(ApiItsm.ValidateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDevices.RecalculateComplianceScansResponse();
        foreach (var item in (request.ChangeRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiDevices.RecalculateComplianceScansRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    OccurredAt = (item.ChangedAt ?? new Timestamp()),
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        DisplayName = (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiItsm.ValidateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = (request.ChangeRequest?.Assignee ?? new ApiIdentity.UserRef()),
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ValidateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
}
