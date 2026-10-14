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
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class OfferApprovalEndpoint
{
    /// <summary>
    /// Approves an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ApproveOfferResponse> ApproveOffer(ApiRecruiting.ApproveOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Offer?.Audit?.Etag ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Offer?.BillingAddress?.CountryCode ?? ""),
                    Locality = (request.Offer?.BillingAddress?.Locality ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.CloneEmployeeRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeService.CloneEmployeeAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ApproveOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.ApproveOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ApproveOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.DelegateOfferApprovalResponse> DelegateOfferApproval(ApiRecruiting.DelegateOfferApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateOfferApproval for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiRecruiting.DelegateOfferApprovalRequest, ApiWorkplaces.CommentDeskApprovalRequest>(value => new ApiWorkplaces.CommentDeskApprovalRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (value.Offer?.BillingAddress?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.Offer?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesDeskApprovalService.CommentDeskApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.DelegateOfferApprovalResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Notes = (request.Offer?.Notes ?? ""),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.DelegateOfferApprovalResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "DelegateOfferApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.EscalateOfferResponse> EscalateOffer(ApiRecruiting.EscalateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.CreateEmployeeResponse();
        foreach (var item in (request.Offer?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.OfferLine>()))
        {
            var input0 = new ApiEmployees.CreateEmployeeRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Employee = new ApiEmployees.Employee
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                    PrimaryAddress = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.Offer?.BillingAddress?.Recipient ?? ""),
                    },
                },
            };
            using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiDocuments.StreamSignatureNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamSignatureNoticesRequest.Types.Filter
            {
                Region = (reply0.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream1 = _documentsSignatureNotificationService.StreamSignatureNotices(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSignatureNotices frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.EscalateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = (request.Offer?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "EscalateOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an offer.
    /// </summary>
    public override async Task<ApiRecruiting.GetOfferApprovalStatusResponse> GetOfferApprovalStatus(ApiRecruiting.GetOfferApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling GetOfferApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "GetOfferApprovalStatus", "accepted", token);
        var response = new ApiRecruiting.GetOfferApprovalStatusResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                OfferId = (request.OfferId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists offers a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListPendingOffersResponse> ListPendingOffers(ApiRecruiting.ListPendingOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingOffers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", "batch",
            "ListPendingOffers", "accepted", token);
        var input0 = new ApiRecruiting.SyncOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SyncOffersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingOfferSyncService.SyncOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SyncOffersRequest>("acme.recruiting.v1.OfferApprovalService/ListPendingOffers/0", input0.Clone()), token);
        var input1 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (reply0.Offer?.BillingAddress?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesEmployeeService.SearchEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ListPendingOffersResponse
        {
            Offers = { new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Offer?.BillingAddress?.Recipient ?? ""),
                },
            } },
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply1.PageInfo?.TotalSize ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ListPendingOffers", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an offer.
    /// </summary>
    public override async Task<ApiRecruiting.RecallOfferResponse> RecallOffer(ApiRecruiting.RecallOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SuspendOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SuspendOfferRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Offer?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
            OfferId = (request.Offer?.OfferId ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.SuspendOfferRequest>();
        using var rpc0 = _recruitingOfferLifecycleService.SuspendOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.RecallOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.RecallOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "RecallOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an offer.
    /// </summary>
    public override async Task<ApiRecruiting.RejectOfferResponse> RejectOffer(ApiRecruiting.RejectOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Offer?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.OfferLine>()).Select(async item =>
        {
            var input0 = new ApiEmployees.CalculateEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Lines = { new ApiEmployees.EmploymentContractLine
                    {
                        Tax = new ApiEmployees.EmploymentContractLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Jurisdiction = (item.Tax?.Jurisdiction ?? ""),
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc0 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiEmployees.CalculateEmploymentContractResponse();
        var response = new ApiRecruiting.RejectOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = (request.Offer?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "RejectOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an offer.
    /// </summary>
    public override async Task<ApiRecruiting.SubmitOfferResponse> SubmitOffer(ApiRecruiting.SubmitOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CreateEmployeeRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Employee = new ApiEmployees.Employee
        {
            RelationshipManager = new ApiIdentity.UserRef
            {
                DisplayName = (request.Offer?.Counterparty?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.SubmitOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = (request.Offer?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                Number = (reply0.Employee?.NationalId?.Number ?? ""),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "SubmitOffer", (request.Reason ?? ""), token);
        return response;
    }
}
