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
using ApiAudit = Acme.Audit.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class JobOpeningEndpoint
{
    /// <summary>
    /// Takes a stream of job openings and answers once at the end.
    /// </summary>
    public override async Task<ApiRecruiting.BatchCreateJobOpeningsResponse> BatchCreateJobOpenings(IAsyncStreamReader<ApiRecruiting.BatchCreateJobOpeningsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiRecruiting.BatchCreateJobOpeningsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateJobOpenings for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiMetrics.BrowseMetricSeriesesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.CreatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    Labels = { request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()),
                        _ => new Google.Protobuf.Collections.MapField<string, string>()
                    } },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.Counterparty?.InterviewId ?? ""),
                        _ => ""
                    },
                    Region = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.BillingAddress?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc0 = _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiRecruiting.CountCandidatesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiRecruiting.CountCandidatesRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.CreatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.Counterparty?.InterviewId ?? ""),
                        _ => ""
                    },
                    Region = request.ChunkCase switch
                    {
                        ApiRecruiting.BatchCreateJobOpeningsRequest.ChunkOneofCase.Record => (request.Record?.BillingAddress?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc1 = _recruitingCandidateQueryService.CountCandidatesAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiRecruiting.BatchCreateJobOpeningsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.VoucherCode ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "job_opening",
                "audit", "BatchCreateJobOpenings", (request.Record?.VoucherCode ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several job openings at once.
    /// </summary>
    public override async Task<ApiRecruiting.BatchGetJobOpeningsResponse> BatchGetJobOpenings(ApiRecruiting.BatchGetJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "BatchGetJobOpenings", "accepted", token);
        var input0 = new ApiAudit.NormalizeAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Source = (request.Context?.Actor?.UserId ?? ""),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.NormalizeAuditEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _auditAuditEventValidationService.NormalizeAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.BatchGetJobOpeningsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Creates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CloneJobOpeningResponse> CloneJobOpening(ApiRecruiting.CloneJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CloneJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CountEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.CountEnrollmentsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
                Labels = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.JobOpening?.JobOpeningId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiBenefits.CountEnrollmentsRequest>(json0);
        using var rpc0 = _benefitsEnrollmentQueryService.CountEnrollmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateInterviewRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiRecruiting.SimulateInterviewRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                CountryCode = (request.JobOpening?.BillingAddress?.CountryCode ?? ""),
                PostalCode = (request.JobOpening?.BillingAddress?.PostalCode ?? ""),
            },
        };
        input1.Interview = new ApiRecruiting.Interview
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
            },
            Location = new ApiCommon.PostalAddress
            {
                Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
            },
        };
        using var rpc1 = _recruitingInterviewCalculationService.SimulateInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.RecalculateBuildingsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply1.Interview?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (reply1.Interview?.Location?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _workplacesBuildingCalculationService.RecalculateBuildingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.CloneJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = (request.JobOpening?.Counterparty ?? new ApiRecruiting.InterviewRef()),
                IssuedAt = (request.JobOpening?.IssuedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CloneJobOpening", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CreateJobOpeningResponse> CreateJobOpening(ApiRecruiting.CreateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    Number = (request.JobOpening?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLabeling.SimulateLabelingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LabelingTask = new ApiLabeling.LabelingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.JobOpening?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _labelingLabelingTaskCalculationService.SimulateLabelingTaskAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply2 = new ApiRecruiting.LintJobOpeningsResponse();
        foreach (var item in (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()))
        {
            var input2 = new ApiRecruiting.LintJobOpeningsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc2 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiRecruiting.CreateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.LabelingTask?.Audit?.Etag ?? ""),
                },
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                IssuedAt = (request.JobOpening?.IssuedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CreateJobOpening", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.DeleteJobOpeningResponse> DeleteJobOpening(ApiRecruiting.DeleteJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling DeleteJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "DeleteJobOpening", "accepted", token);
        var input0 = new ApiRecruiting.NormalizeOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _sendDeleteJobOpening0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Offer?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Offer?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiRecruiting.OfferCardSettlement
                {
                    HolderName = (reply0.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (reply0.Offer?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Token = (reply0.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (reply0.Offer?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingOfferValidationService.PreviewOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.DeleteJobOpeningResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.JobOpeningId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "DeleteJobOpening", (request.JobOpeningId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.GetJobOpeningResponse> GetJobOpening(ApiRecruiting.GetJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling GetJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "GetJobOpening", "accepted", token);
        var input0 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintOffersRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiRecruiting.LintOffersRequest, ApiRecruiting.LintOffersResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".OfferValidationService", "LintOffers",
            Wire.Marshaller<ApiRecruiting.LintOffersRequest>(), Wire.Marshaller<ApiRecruiting.LintOffersResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Offer?.BillingAddress?.Locality ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListJobOpeningsResponse> ListJobOpenings(ApiRecruiting.ListJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "ListJobOpenings", "accepted", token);
        var input0 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateInterviewRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.EstimateInterviewRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingInterviewCalculationService.EstimateInterviewAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListJobOpeningsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ListJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.SearchJobOpeningsResponse> SearchJobOpenings(ApiRecruiting.SearchJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "SearchJobOpenings", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.SearchJobOpeningsRequest, ApiRecruiting.CheckCandidateEligibilityRequest>(value => new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWebhooks.FindWebhookEndpointsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWebhooks.FindWebhookEndpointsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _webhooksWebhookEndpointQueryService.FindWebhookEndpointsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SearchJobOpeningsResponse
        {
            JobOpenings = { new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            } },
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply1.PageInfo?.NextPageToken ?? ""),
                TotalSize = (reply1.PageInfo?.TotalSize ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SearchJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.UpdateJobOpeningResponse> UpdateJobOpening(ApiRecruiting.UpdateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.UpdateJobOpeningToFindCandidates0();
        using var rpc0 = _recruitingCandidateQueryService.FindCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Card = new ApiRecruiting.OfferCardSettlement
                {
                    Token = (request.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (request.JobOpening?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingOfferValidationService.LintOffersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                Region = (reply1.Offer?.BillingAddress?.Region ?? ""),
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.GetInterviewRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            InterviewId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiRecruiting.GetInterviewRatesRequest>();
        using var rpc3 = _recruitingInterviewCalculationService.GetInterviewRatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.UpdateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply3.Interview?.Location?.Recipient ?? ""),
                },
                Counterparty = (request.JobOpening?.Counterparty ?? new ApiRecruiting.InterviewRef()),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Notes = (reply1.Offer?.Notes ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.JobOpening?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "UpdateJobOpening", (request.JobOpening?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of job openings as they happen.
    /// </summary>
    public override async Task WatchJobOpenings(ApiRecruiting.WatchJobOpeningsRequest request, IServerStreamWriter<ApiRecruiting.WatchJobOpeningsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "WatchJobOpenings", "accepted", token);
        var input0 = new ApiDocuments.SuggestDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _documentsDocumentQueryService.SuggestDocumentsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.LintInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Subscriber?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _recruitingInterviewValidationService.LintInterviewsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.SuggestCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SuggestCandidatesRequest.Types.Filter
            {
                Region = (reply1.Interview?.Location?.Region ?? ""),
            },
        };
        using var rpc2 = _recruitingCandidateQueryService.SuggestCandidatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SuggestCandidatesRequest>("acme.recruiting.v1.JobOpeningService/WatchJobOpenings/2", input2.Clone()), token);
        var input3 = new ApiDevices.ResolveDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyQueryService.ResolveDevicePoliciesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.WatchJobOpeningsResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "WatchJobOpenings", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
