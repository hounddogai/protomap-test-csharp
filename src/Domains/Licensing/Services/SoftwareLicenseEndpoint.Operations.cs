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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class SoftwareLicenseEndpoint
{
    /// <summary>
    /// Takes a stream of software licenses and answers once at the end.
    /// </summary>
    public override async Task<ApiLicensing.BatchCreateSoftwareLicensesResponse> BatchCreateSoftwareLicenses(IAsyncStreamReader<ApiLicensing.BatchCreateSoftwareLicensesRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiLicensing.BatchCreateSoftwareLicensesResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateSoftwareLicenses for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var reply0 = new ApiLicensing.RecalculateSoftwareLicensesResponse();
            foreach (var item in (request.Record?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()))
            {
                var input0 = new ApiLicensing.RecalculateSoftwareLicensesRequest
                {
                    Context = (request.Context ?? new ApiCommon.RequestContext()),
                    SoftwareLicense = new ApiLicensing.SoftwareLicense
                    {
                        Bank = new ApiLicensing.SoftwareLicenseBankFunding
                        {
                            Bic = (request.Record?.Bank?.Bic ?? ""),
                        },
                        Limits = { new ApiLicensing.SoftwareLicenseLimit
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                            Name = (item.Name ?? ""),
                            Period = (item.Period ?? new Duration()),
                        } },
                    },
                };
                using var rpc0 = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input0, cancellationToken: token);
                reply0 = await rpc0.ResponseAsync;
            }
            var response = new ApiLicensing.BatchCreateSoftwareLicensesResponse();
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.SoftwareLicenseId ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "software_license",
                "audit", "BatchCreateSoftwareLicenses", (request.Record?.SoftwareLicenseId ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several software licenses at once.
    /// </summary>
    public override async Task<ApiLicensing.BatchGetSoftwareLicensesResponse> BatchGetSoftwareLicenses(ApiLicensing.BatchGetSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "BatchGetSoftwareLicenses", "accepted", token);
        var input0 = new ApiLicensing.EstimateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.EstimateSoftwareLicenseRequest>();
        using var rpc0 = _licensingSoftwareLicenseCalculationService.EstimateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.BatchGetSoftwareLicensesResponse
        {
            SoftwareLicenses = { new ApiLicensing.SoftwareLicense
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            } },
        };
        return response;
    }
    /// <summary>
    /// Creates a software license.
    /// </summary>
    public override async Task<ApiLicensing.CloneSoftwareLicenseResponse> CloneSoftwareLicense(ApiLicensing.CloneSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CloneSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.SoftwareLicense?.Contact?.Phone ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    Number = (request.SoftwareLicense?.Holder?.Number ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.SimulateSoftwareLicenseRequest>(json0);
        using var rpc0 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.SoftwareLicense?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CloneSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                OpenedAt = (request.SoftwareLicense?.OpenedAt ?? new Timestamp()),
                SoftwareLicenseId = (reply0.SoftwareLicense?.SoftwareLicenseId ?? ""),
                Status = (request.SoftwareLicense?.Status ?? (ApiLicensing.SoftwareLicenseStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CloneSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a software license.
    /// </summary>
    public override async Task<ApiLicensing.CreateSoftwareLicenseResponse> CreateSoftwareLicense(ApiLicensing.CreateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.NormalizeEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _licensingEntitlementValidationService.NormalizeEntitlementAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExplainEntitlementDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.ExplainEntitlementDecisionAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiLicensing.CreateSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = (request.SoftwareLicense?.Contact ?? new ApiCommon.ContactInfo()),
                Holder = (request.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CreateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.GetSoftwareLicenseResponse> GetSoftwareLicense(ApiLicensing.GetSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        _logger.LogInformation("Handling GetSoftwareLicense for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "GetSoftwareLicense", "accepted", token);
        var response = new ApiLicensing.GetSoftwareLicenseResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.ListSoftwareLicensesResponse> ListSoftwareLicenses(ApiLicensing.ListSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "ListSoftwareLicenses", "accepted", token);
        var input0 = new ApiDocuments.WatchDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.WatchDocumentsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _documentsDocumentService.WatchDocuments(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDocuments frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLicensing.LintLicenseAssignmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.LintLicenseAssignmentsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingLicenseAssignmentValidationService.LintLicenseAssignmentsAsync);
        var response = new ApiLicensing.ListSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ListSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.SearchSoftwareLicensesResponse> SearchSoftwareLicenses(ApiLicensing.SearchSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "SearchSoftwareLicenses", "accepted", token);
        var input0 = new ApiLicensing.PreviewLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PreviewLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendSearchSoftwareLicenses0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                    Region = (reply0.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Last4 = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (reply0.LicenseAssignment?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.SearchSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicenses = { new ApiLicensing.SoftwareLicense
            {
                Holder = (reply1.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SearchSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a software license.
    /// </summary>
    public override async Task<ApiLicensing.UpdateSoftwareLicenseResponse> UpdateSoftwareLicense(ApiLicensing.UpdateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.PreviewSoftwareLicenseResponse();
        foreach (var item in (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiLicensing.PreviewSoftwareLicenseRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Limits = { new ApiLicensing.SoftwareLicenseLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc0 = _licensingSoftwareLicenseValidationService.PreviewSoftwareLicenseAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiNotifications.NormalizeNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.SoftwareLicense?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Comments = { new ApiNotifications.NotificationComment
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        using var rpc1 = _notificationsNotificationValidationService.NormalizeNotificationAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiNotifications.CountNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.CountNotificationsRequest.Types.Filter
            {
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc2 = _notificationsNotificationQueryService.CountNotificationsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiLicensing.UpdateSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Balance = (request.SoftwareLicense?.Balance ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Notification?.Requester?.Email ?? ""),
                },
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "UpdateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of software licenses as they happen.
    /// </summary>
    public override async Task WatchSoftwareLicenses(ApiLicensing.WatchSoftwareLicensesRequest request, IServerStreamWriter<ApiLicensing.WatchSoftwareLicensesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "WatchSoftwareLicenses", "accepted", token);
        var input0 = new ApiLicensing.PreviewSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PreviewSoftwareLicenseRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Phone = (request.Subscriber?.Phone ?? ""),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingSoftwareLicenseValidationService.PreviewSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.PreviewSoftwareLicenseRequest>("acme.licensing.v1.SoftwareLicenseService/WatchSoftwareLicenses/0", input0.Clone()), token);
        var input1 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Bank = new ApiLicensing.LicenseAssignmentBankSettlement
                {
                    Bic = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply0.SoftwareLicense?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                    Iban = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply0.SoftwareLicense?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.WatchSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                    PreferredChannel = (reply0.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    Number = (reply1.LicenseAssignment?.Number ?? ""),
                },
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "WatchSoftwareLicenses", (request.Filter?.Region ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
