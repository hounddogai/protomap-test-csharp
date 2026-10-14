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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class InterviewCalculationEndpoint
{
    /// <summary>
    /// Calculates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.CalculateInterviewResponse> CalculateInterview(ApiRecruiting.CalculateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Interview?.Location?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Interview?.Location?.Recipient ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiRecruiting.QuoteJobOpeningRequest, ApiRecruiting.QuoteJobOpeningResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningCalculationService", "QuoteJobOpening",
            Wire.Marshaller<ApiRecruiting.QuoteJobOpeningRequest>(), Wire.Marshaller<ApiRecruiting.QuoteJobOpeningResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainJobOpeningCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.ExplainJobOpeningCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingJobOpeningCalculationService.ExplainJobOpeningCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiRecruiting.CalculateInterviewRequest, ApiChat.CalculateChatRoomRequest>(value => new ApiChat.CalculateChatRoomRequest
        {
            ChatRoom = new ApiChat.ChatRoom
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (value.Details?.Destination?.Region ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _chatChatRoomCalculationService.CalculateChatRoomAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiPrescriptions.FindPrescriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPrescriptions.FindPrescriptionsRequest.Types.Filter
            {
                CreatedAfter = (reply0.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
                OwnerId = (reply2.ChatRoom?.Owner?.UserId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _prescriptionsPrescriptionQueryService.FindPrescriptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.CalculateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                },
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                Organizer = (request.Interview?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "CalculateInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an interview.
    /// </summary>
    public override async Task<ApiRecruiting.CompareInterviewOptionsResponse> CompareInterviewOptions(ApiRecruiting.CompareInterviewOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareInterviewOptions for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CompareInterviewOptionsToPreviewItinerary0();
        using var rpc0 = _travelItineraryValidationService.PreviewItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScoreItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await SendCompareInterviewOptions1Async(input1, token);
        var response = new ApiRecruiting.CompareInterviewOptionsResponse
        {
            Breakdown = new ApiRecruiting.CompareInterviewOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Itinerary?.Audit?.Etag ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "CompareInterviewOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.EstimateInterviewResponse> EstimateInterview(ApiRecruiting.EstimateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.JobOpening?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiRecruiting.SimulateJobOpeningRequest>();
        using var rpc1 = _recruitingJobOpeningCalculationService.SimulateJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.EstimateInterviewResponse
        {
            Breakdown = new ApiRecruiting.EstimateInterviewResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                InterviewId = (request.InterviewId ?? ""),
                Metadata = { (reply1.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (request.Interview?.Status ?? (ApiRecruiting.InterviewStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "EstimateInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainInterviewCalculationResponse> ExplainInterviewCalculation(ApiRecruiting.ExplainInterviewCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainInterviewCalculation for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Interview?.Location?.PostalCode ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiRecruiting.RecalculateJobOpeningsRequest>(json0);
        using var rpc0 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    Iban = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Bank => (reply0.JobOpening?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.JobOpening?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SimulateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _recruitingCandidateCalculationService.SimulateCandidateAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiSearch.SuggestSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply1.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexQueryService.SuggestSearchIndexesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ExplainInterviewCalculationResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (reply2.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                Organizer = (request.Interview?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiRecruiting.ExplainInterviewCalculationResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ExplainInterviewCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GetInterviewRatesResponse> GetInterviewRates(ApiRecruiting.GetInterviewRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling GetInterviewRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GetInterviewRates", "accepted", token);
        var input0 = new ApiDocuments.ReconcileDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                    },
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Children = { new ApiDocuments.DocumentFolder
                        {
                            ParentId = (request.InterviewId ?? ""),
                        } },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Context?.Actor?.Email ?? ""),
                        },
                    } },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetInterviewRatesResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.RecalculateInterviewsResponse> RecalculateInterviews(ApiRecruiting.RecalculateInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateInterviews for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ResolveSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseQueryService.ResolveSoftwareLicensesAsync);
        var reply1 = new ApiHelpdesk.FindKnowledgeArticlesResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiHelpdesk.FindKnowledgeArticlesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiHelpdesk.FindKnowledgeArticlesRequest.Types.Filter
                {
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc1 = _helpdeskKnowledgeArticleQueryService.FindKnowledgeArticlesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CalculateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Interview?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>("acme.recruiting.v1.JobOpeningCalculationService");
        using var rpc2 = named2.CalculateJobOpeningAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.RecalculateInterviewsResponse
        {
            Breakdown = new ApiRecruiting.RecalculateInterviewsResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
                Total = (reply2.JobOpening?.Total ?? new ApiCommon.Money()),
            },
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "RecalculateInterviews", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.SimulateInterviewResponse> SimulateInterview(ApiRecruiting.SimulateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Interview?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Interview?.Location?.Recipient ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiRecruiting.EstimateJobOpeningRequest, ApiRecruiting.EstimateJobOpeningResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningCalculationService", "EstimateJobOpening",
            Wire.Marshaller<ApiRecruiting.EstimateJobOpeningRequest>(), Wire.Marshaller<ApiRecruiting.EstimateJobOpeningResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CountKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.CountKnowledgeArticlesRequest.Types.Filter
            {
                CreatedAfter = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Interview?.InterviewId ?? ""),
                Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiHelpdesk.CountKnowledgeArticlesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _helpdeskKnowledgeArticleQueryService.CountKnowledgeArticlesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SimulateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "SimulateInterview", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.ScoreItineraryResponse> SendCompareInterviewOptions1Async(ApiTravel.ScoreItineraryRequest request, CancellationToken token)
    {
        return await _interviewCalculationEndpointSendCompareInterviewOptions1Port.SendAsync(request, token);
    }
}
