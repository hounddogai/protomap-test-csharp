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
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSeating = Acme.Seating.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingTaskCalculationEndpoint
{
    /// <summary>
    /// Calculates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.CalculateOnboardingTaskResponse> CalculateOnboardingTask(ApiOnboarding.CalculateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiDocuments.ListSignatureVersionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc0 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.ListSignatureVersionsResponse();
        var input1 = new ApiTimekeeping.CheckClockEventEligibilityRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CheckClockEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.CheckClockEventEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CalculateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                CustomFields = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.ClockEvent?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CalculateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.CompareOnboardingTaskOptionsResponse> CompareOnboardingTaskOptions(ApiOnboarding.CompareOnboardingTaskOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareOnboardingTaskOptions for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CompareOnboardingPlanOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CompareOnboardingPlanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.CompareOnboardingPlanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RecalculateSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateSignaturesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.RecalculateSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSeating.ResolveSeatMapsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _seatingSeatMapQueryService.ResolveSeatMapsAsync);
        var input3 = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingTask?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _sendCompareOnboardingTaskOptions3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.CompareOnboardingTaskOptionsResponse
        {
            Breakdown = new ApiOnboarding.CompareOnboardingTaskOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Status = (request.OnboardingTask?.Status ?? (ApiOnboarding.OnboardingTaskStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CompareOnboardingTaskOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.EstimateOnboardingTaskResponse> EstimateOnboardingTask(ApiOnboarding.EstimateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiMetrics.CloneMetricSeriesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                MetricSeries = new ApiMetrics.MetricSeries
                {
                    Address = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    },
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc0 = _metricsMetricSeriesService.CloneMetricSeriesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiMetrics.CloneMetricSeriesResponse();
        var response = new ApiOnboarding.EstimateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                CustomFields = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "EstimateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainOnboardingTaskCalculationResponse> ExplainOnboardingTaskCalculation(ApiOnboarding.ExplainOnboardingTaskCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainOnboardingTaskCalculation for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CheckLifeEventEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CheckLifeEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _benefitsLifeEventValidationService.CheckLifeEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ExplainOnboardingTaskCalculationResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                CustomFields = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.ExplainOnboardingTaskCalculationResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ExplainOnboardingTaskCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingTaskRatesResponse> GetOnboardingTaskRates(ApiOnboarding.GetOnboardingTaskRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling GetOnboardingTaskRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "GetOnboardingTaskRates", "accepted", token);
        var input0 = new ApiOnboarding.CompareOnboardingPlanOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
            },
        };
        var command0 = new DispatchCommand<ApiOnboarding.CompareOnboardingPlanOptionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.CompareOnboardingPlanOptionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetOnboardingTaskRatesResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Comments = { new ApiOnboarding.OnboardingTaskComment
                {
                    CreatedAt = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.QuoteOnboardingTaskResponse> QuoteOnboardingTask(ApiOnboarding.QuoteOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiVenues.UpdateVenueRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Venue = new ApiVenues.Venue
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc0 = _venuesVenueService.UpdateVenueAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiVenues.UpdateVenueResponse();
        var reply1 = new ApiOnboarding.QuoteOnboardingPlanResponse();
        foreach (var item in (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()))
        {
            var input1 = new ApiOnboarding.QuoteOnboardingPlanRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiOnboarding.QuoteOnboardingPlanRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Region = (reply0.Venue?.Address?.Region ?? ""),
                    },
                },
                OnboardingPlan = new ApiOnboarding.OnboardingPlan
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc1 = _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiOnboarding.EstimateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.EstimateOnboardingPlanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    PostalCode = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                    {
                        ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Bic = (reply1.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (reply1.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.EstimateOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input3 = new ApiDevices.NormalizeDevicePolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DevicePolicy = new ApiDevices.DevicePolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc3 = _devicesDevicePolicyValidationService.NormalizeDevicePolicyAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiDevices.NormalizeDevicePolicyResponse();
        var response = new ApiOnboarding.QuoteOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (request.OnboardingTask?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "QuoteOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.RecalculateOnboardingTasksResponse> RecalculateOnboardingTasks(ApiOnboarding.RecalculateOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateOnboardingTasks for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CalculateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    Code = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.Code ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.CalculateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                    Number = (reply0.OnboardingPlan?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.OnboardingTask?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.RecalculateOnboardingTasksResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "RecalculateOnboardingTasks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.SimulateOnboardingTaskResponse> SimulateOnboardingTask(ApiOnboarding.SimulateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskStatusChange>()).Select(async item =>
        {
            var input0 = new ApiDocuments.ListDocumentVersionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentVersionsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (request.OnboardingTask?.Sla?.PolicyId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc0 = _documentsDocumentHistoryService.ListDocumentVersionsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.ListDocumentVersionsResponse();
        var reply1 = new ApiOnboarding.CalculateOnboardingPlanResponse();
        foreach (var item in (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()))
        {
            var input1 = new ApiOnboarding.CalculateOnboardingPlanRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiOnboarding.CalculateOnboardingPlanRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                        Recipient = (request.Details?.Destination?.Recipient ?? ""),
                    },
                },
                OnboardingPlan = new ApiOnboarding.OnboardingPlan
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc1 = _onboardingOnboardingPlanCalculationService.CalculateOnboardingPlanAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiOnboarding.SimulateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                CustomFields = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SimulateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
}
