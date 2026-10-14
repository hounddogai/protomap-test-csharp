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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class LifeEventCalculationEndpoint
{
    /// <summary>
    /// Calculates a life event.
    /// </summary>
    public override async Task<ApiBenefits.CalculateLifeEventResponse> CalculateLifeEvent(ApiBenefits.CalculateLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CheckFeedbackEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Kind = (request.LifeEvent?.Kind ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (request.LifeEvent?.Subject?.CandidateId ?? ""),
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackValidationService.CheckFeedbackEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiHelpdesk.ExplainTicketCalculationResponse();
        foreach (var item in (request.LifeEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.LifeEventMeasurement>()))
        {
            var input1 = new ApiHelpdesk.ExplainTicketCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Ticket = new ApiHelpdesk.Ticket
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        Email = (request.LifeEvent?.Subject?.Email ?? ""),
                    },
                    Customer = new ApiHelpdesk.KnowledgeArticleRef
                    {
                        DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                    },
                    Requester = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Feedback?.Subject?.Email ?? ""),
                    },
                    Sla = new ApiHelpdesk.TicketSla
                    {
                        DueAt = (item.ObservedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc1 = _helpdeskTicketCalculationService.ExplainTicketCalculationAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiBenefits.CalculateLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                OccurredAt = (reply0.Feedback?.OccurredAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.LifeEvent?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "CalculateLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a life event.
    /// </summary>
    public override async Task<ApiBenefits.CompareLifeEventOptionsResponse> CompareLifeEventOptions(ApiBenefits.CompareLifeEventOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareLifeEventOptions for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementCalculationService.QuoteEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ValidateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply0.Entitlement?.Owner?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmProblemValidationService.ValidateProblemAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.CompareLifeEventOptionsResponse
        {
            Breakdown = new ApiBenefits.CompareLifeEventOptionsResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Source = (request.LifeEvent?.Source ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.Problem?.Assignee?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "CompareLifeEventOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a life event.
    /// </summary>
    public override async Task<ApiBenefits.EstimateLifeEventResponse> EstimateLifeEvent(ApiBenefits.EstimateLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListSignatureConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.LifeEvent?.OccurredAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsSignatureSyncService.ListSignatureConflictsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply1 = new ApiFeatureflags.ListFeatureFlagsResponse();
        foreach (var item in (request.LifeEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.LifeEventMeasurement>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiFeatureflags.ListFeatureFlagsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    OwnerId = (request.LifeEventId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc1 = _featureflagsFeatureFlagService.ListFeatureFlagsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiLearning.RecalculateCertificationsRequest
        {
            Certification = new ApiLearning.Certification
            {
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _sendEstimateLifeEvent2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.ResolveBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named3 = _registry.Get<ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient>("acme.workplaces.v1.BuildingQueryService");
        using var rpc3 = named3.ResolveBuildingsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.EstimateLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = (request.LifeEvent?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "EstimateLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a life event.
    /// </summary>
    public override async Task<ApiBenefits.ExplainLifeEventCalculationResponse> ExplainLifeEventCalculation(ApiBenefits.ExplainLifeEventCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainLifeEventCalculation for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.NormalizeTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.NormalizeTravelBookingRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var descriptor0 = new Method<ApiTravel.NormalizeTravelBookingRequest, ApiTravel.NormalizeTravelBookingResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TravelBookingValidationService", "NormalizeTravelBooking",
            Wire.Marshaller<ApiTravel.NormalizeTravelBookingRequest>(), Wire.Marshaller<ApiTravel.NormalizeTravelBookingResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.TravelBooking?.Audit?.Etag ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Token = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply0.TravelBooking?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentValidationService.ValidateDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ExplainLifeEventCalculationResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.ExplainLifeEventCalculationResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ExplainLifeEventCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a life event.
    /// </summary>
    public override async Task<ApiBenefits.GetLifeEventRatesResponse> GetLifeEventRates(ApiBenefits.GetLifeEventRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LifeEventId, "life_event_id");
        _logger.LogInformation("Handling GetLifeEventRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "life_event", request.LifeEventId,
            "GetLifeEventRates", "accepted", token);
        var profile0 = new MappingProfile<ApiBenefits.GetLifeEventRatesRequest, ApiFeatureflags.BatchGetFeatureFlagsRequest>(value => new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamSignatureExportRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        using var stream1 = _documentsSignatureExportService.StreamSignatureExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSignatureExport frame {Type}", change1.GetType().Name);
        }
        var response = new ApiBenefits.GetLifeEventRatesResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a life event.
    /// </summary>
    public override async Task<ApiBenefits.QuoteLifeEventResponse> QuoteLifeEvent(ApiBenefits.QuoteLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.QuoteLifeEventToBatchGetPrescriptions0();
        using var rpc0 = _prescriptionsPrescriptionService.BatchGetPrescriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.QuoteLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Source = (request.LifeEvent?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "QuoteLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a life event.
    /// </summary>
    public override async Task<ApiBenefits.RecalculateLifeEventsResponse> RecalculateLifeEvents(ApiBenefits.RecalculateLifeEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateLifeEvents for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SimulateTravelBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var reply0 = await SendRecalculateLifeEvents0Async(input0, token);
        var input1 = new ApiPrescriptions.UpdatePrescriptionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Prescription = new ApiPrescriptions.Prescription
            {
                Bank = new ApiPrescriptions.PrescriptionBankSettlement
                {
                    AccountHolder = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply0.TravelBooking?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _prescriptionsPrescriptionService.UpdatePrescriptionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.RecalculateLifeEventsResponse
        {
            Breakdown = new ApiBenefits.RecalculateLifeEventsResponse.Types.Breakdown
            {
                Total = (reply0.TravelBooking?.Total ?? new ApiCommon.Money()),
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Sequence = (request.LifeEvent?.Sequence ?? 0L),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "RecalculateLifeEvents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a life event.
    /// </summary>
    public override async Task<ApiBenefits.SimulateLifeEventResponse> SimulateLifeEvent(ApiBenefits.SimulateLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.LifeEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.LifeEventMeasurement>()))
        {
            var input0 = new ApiDocuments.StreamDocumentFolderExportRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.StreamDocumentFolderExportRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var stream0 = _documentsDocumentFolderExportService.StreamDocumentFolderExport(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received StreamDocumentFolderExport frame {Type}", change0.GetType().Name);
            }
        }
        var input1 = new ApiLedger.CountLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.CountLedgerAccountsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLedger.CountLedgerAccountsRequest>(json1);
        using var rpc1 = _ledgerLedgerAccountQueryService.CountLedgerAccountsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.SimulateLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "SimulateLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.SimulateTravelBookingResponse> SendRecalculateLifeEvents0Async(ApiTravel.SimulateTravelBookingRequest request, CancellationToken token)
    {
        return await _lifeEventCalculationEndpointSendRecalculateLifeEvents0Port.SendAsync(request, token);
    }
}
