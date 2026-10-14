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
using ApiAdmissions = Acme.Admissions.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class RosterCalculationEndpoint
{
    /// <summary>
    /// Calculates a roster.
    /// </summary>
    public override async Task<ApiScheduling.CalculateRosterResponse> CalculateRoster(ApiScheduling.CalculateRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudiences.WatchAudiencesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Roster?.Counterparty?.Email ?? ""),
            },
        };
        using var stream0 = _audiencesAudienceService.WatchAudiences(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAudiences frame {Type}", change0.GetType().Name);
        }
        var response = new ApiScheduling.CalculateRosterResponse
        {
            Roster = new ApiScheduling.Roster
            {
                RosterId = (request.RosterId ?? ""),
                Total = (request.Roster?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CalculateRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a roster.
    /// </summary>
    public override async Task<ApiScheduling.CompareRosterOptionsResponse> CompareRosterOptions(ApiScheduling.CompareRosterOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareRosterOptions for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PreviewFeedbackRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Roster?.Counterparty?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _performanceFeedbackValidationService.PreviewFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPrescriptions.QuotePrescriptionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPrescriptions.QuotePrescriptionRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Roster?.BillingAddress?.Recipient ?? ""),
                },
            },
            Prescription = new ApiPrescriptions.Prescription
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Roster?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiPrescriptions.QuotePrescriptionRequest>(json1);
        using var rpc1 = _prescriptionsPrescriptionCalculationService.QuotePrescriptionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.CompareRosterOptionsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = (request.Roster?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CompareRosterOptions", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a roster.
    /// </summary>
    public override async Task<ApiScheduling.EstimateRosterResponse> EstimateRoster(ApiScheduling.EstimateRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiAdmissions.ListAdmissionsResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()))
        {
            var input1 = new ApiAdmissions.ListAdmissionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiAdmissions.ListAdmissionsRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.LineId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc1 = _admissionsAdmissionService.ListAdmissionsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiHelpdesk.FindTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.FindTicketsRequest.Types.Filter
            {
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskTicketQueryService.FindTicketsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var batch3 = (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()).Select(async item =>
        {
            var input3 = new ApiMetrics.PreviewMetricSeriesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                MetricSeries = new ApiMetrics.MetricSeries
                {
                    Address = new ApiCommon.PostalAddress
                    {
                        Locality = (request.Details?.Destination?.Locality ?? ""),
                    },
                    Components = { { "primary", new ApiMetrics.MetricSeriesComponent
                    {
                        Description = (item.Description ?? ""),
                        Quantity = item.Quantity,
                    } } },
                },
            };
            using var rpc3 = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiMetrics.PreviewMetricSeriesResponse();
        var response = new ApiScheduling.EstimateRosterResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
                IssuedAt = (request.Roster?.IssuedAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "EstimateRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a roster.
    /// </summary>
    public override async Task<ApiScheduling.ExplainRosterCalculationResponse> ExplainRosterCalculation(ApiScheduling.ExplainRosterCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRosterCalculation for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ValidateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ValidateFeedbackRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _performanceFeedbackValidationService.ValidateFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    HolderName = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (request.Roster?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (request.Roster?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.SyncDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.BatchGetManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _devicesManagedDeviceService.BatchGetManagedDevicesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply1.Document?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiScheduling.ExplainRosterCalculationResponse
        {
            Outcome = new ApiScheduling.ExplainRosterCalculationResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                },
                RosterId = (request.RosterId ?? ""),
                Total = (request.Roster?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ExplainRosterCalculation", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a roster.
    /// </summary>
    public override async Task<ApiScheduling.GetRosterRatesResponse> GetRosterRates(ApiScheduling.GetRosterRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RosterId, "roster_id");
        _logger.LogInformation("Handling GetRosterRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "GetRosterRates", "accepted", token);
        var input0 = new ApiLedger.LintLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.LintLedgerAccountsRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLedger.LintLedgerAccountsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _ledgerLedgerAccountValidationService.LintLedgerAccountsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiScheduling.GetRosterRatesRequest, ApiHelpdesk.UpdateTicketRequest>(value => new ApiHelpdesk.UpdateTicketRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LedgerAccount?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply0.LedgerAccount?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.LedgerAccount?.Contact?.Phone ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _helpdeskTicketService.UpdateTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.GetRosterRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RosterId = (request.RosterId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a roster.
    /// </summary>
    public override async Task<ApiScheduling.QuoteRosterResponse> QuoteRoster(ApiScheduling.QuoteRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SendDocumentTemplateTestNoticeRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTemplateTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Roster?.Counterparty?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.SendDocumentTemplateTestNoticeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.QuoteRosterToQuoteDesk1(reply0);
        using var rpc1 = _workplacesDeskCalculationService.QuoteDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.NormalizeChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (reply1.Desk?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await SendQuoteRoster2Async(input2, token);
        var input3 = new ApiAdmissions.SearchAdmissionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAdmissions.SearchAdmissionsRequest.Types.Filter
            {
                CreatedAfter = (reply2.ChangeRequest?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (reply2.ChangeRequest?.Assignee?.UserId ?? ""),
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _admissionsAdmissionService.SearchAdmissionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.QuoteRosterResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Desk?.Address?.Locality ?? ""),
                },
                IssuedAt = (request.Roster?.IssuedAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "QuoteRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a roster.
    /// </summary>
    public override async Task<ApiScheduling.RecalculateRostersResponse> RecalculateRosters(ApiScheduling.RecalculateRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateRosters for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.ScoreSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.ScoreSearchIndexRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Roster?.BillingAddress?.CountryCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiSearch.ScoreSearchIndexRequest>();
        using var rpc0 = _searchSearchIndexValidationService.ScoreSearchIndexAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (reply0.SearchIndex?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.SearchIndex?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.SyncSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.RecalculateRostersResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Signature?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                RosterId = (request.RosterId ?? ""),
                Total = (request.Roster?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "RecalculateRosters", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a roster.
    /// </summary>
    public override async Task<ApiScheduling.SimulateRosterResponse> SimulateRoster(ApiScheduling.SimulateRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateDeskRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Desk = new ApiWorkplaces.Desk
        {
            Address = new ApiCommon.PostalAddress
            {
                Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        input0.Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
            },
        };
        using var rpc0 = _workplacesDeskCalculationService.EstimateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (reply0.Desk?.Dimensions?.Unit ?? ""),
                } },
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.BrowseIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.BrowseIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Desk?.DeskId ?? ""),
                Region = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _itsmIncidentQueryService.BrowseIncidentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.SimulateRosterResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Number = (request.Roster?.Number ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "SimulateRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.NormalizeChangeRequestResponse> SendQuoteRoster2Async(ApiItsm.NormalizeChangeRequestRequest request, CancellationToken token)
    {
        return await _rosterCalculationEndpointSendQuoteRoster2Port.SendAsync(request, token);
    }
}
