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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class CalibrationValidationEndpoint
{
    /// <summary>
    /// Checks a calibration.
    /// </summary>
    public override async Task<ApiPerformance.CheckCalibrationEligibilityResponse> CheckCalibrationEligibility(ApiPerformance.CheckCalibrationEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCalibrationEligibility for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CheckCalibrationEligibilityToGetCalibrationRates0();
        using var rpc0 = _performanceCalibrationCalculationService.GetCalibrationRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.EstimateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Calibration?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await SendCheckCalibrationEligibility1Async(input1, token);
        var input2 = new ApiItsm.LintIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Calibration?.Assignee?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Calibration?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _itsmIncidentValidationService.LintIncidentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiPerformance.CheckFeedbackEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CheckFeedbackEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Calibration?.Requester?.PreferredChannel ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Calibration?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _performanceFeedbackValidationService.CheckFeedbackEligibilityAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.CheckCalibrationEligibilityResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = (reply2.Incident?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Calibration?.Audit?.Etag ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
                Sla = new ApiPerformance.CalibrationSla
                {
                    DueAt = (reply1.Calibration?.Sla?.DueAt ?? new Timestamp()),
                },
                Status = (reply1.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
                Subject = (request.Calibration?.Subject ?? ""),
            },
            Outcome = new ApiPerformance.CheckCalibrationEligibilityResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "CheckCalibrationEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ExplainCalibrationDecisionResponse> ExplainCalibrationDecision(ApiPerformance.ExplainCalibrationDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCalibrationDecision for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Calibration?.Requester?.PreferredChannel ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Calibration?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiDocuments.ValidateDocumentRequest>(json0);
        using var rpc0 = _documentsDocumentValidationService.ValidateDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ExplainCalibrationDecisionResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
                Priority = (request.Calibration?.Priority ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ExplainCalibrationDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a calibration.
    /// </summary>
    public override async Task<ApiPerformance.GetCalibrationRulesResponse> GetCalibrationRules(ApiPerformance.GetCalibrationRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        _logger.LogInformation("Handling GetCalibrationRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "GetCalibrationRules", "accepted", token);
        var input0 = new ApiPerformance.CalculateCalibrationRequest();
        input0.Calibration = new ApiPerformance.Calibration
        {
            Assignee = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        input0.CalibrationId = (request.CalibrationId ?? "");
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _performanceCalibrationCalculationService.CalculateCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SimulateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Sla = new ApiItsm.ChangeRequestSla
                {
                    Breached = (reply0.Calibration?.Sla?.Breached ?? false),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmChangeRequestCalculationService.SimulateChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.GetCalibrationRulesResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = (reply0.Calibration?.Assignee ?? new ApiIdentity.UserRef()),
                CalibrationId = (request.CalibrationId ?? ""),
                Priority = (reply1.ChangeRequest?.Priority ?? 0),
                Tags = { (reply1.ChangeRequest?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a calibration.
    /// </summary>
    public override async Task<ApiPerformance.LintCalibrationsResponse> LintCalibrations(ApiPerformance.LintCalibrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCalibrations for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiWorkplaces.ExplainRoomBookingCalculationResponse();
        foreach (var item in (request.Calibration?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.CalibrationComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                RoomBooking = new ApiWorkplaces.RoomBooking
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Participants = { new ApiWorkplaces.RoomBookingParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.AuthorEmail ?? ""),
                        },
                    } },
                },
            };
            using var rpc0 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiPerformance.LintCalibrationsResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Status = (request.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "LintCalibrations", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a calibration.
    /// </summary>
    public override async Task<ApiPerformance.NormalizeCalibrationResponse> NormalizeCalibration(ApiPerformance.NormalizeCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.LintEscalationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationValidationService.LintEscalationsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.NormalizeCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                CustomFields = { (request.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiPerformance.NormalizeCalibrationResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "NormalizeCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a calibration.
    /// </summary>
    public override async Task<ApiPerformance.PreviewCalibrationResponse> PreviewCalibration(ApiPerformance.PreviewCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiPerformance.LintGoalsResponse();
        foreach (var item in (request.Calibration?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.CalibrationComment>()))
        {
            var input0 = new ApiPerformance.LintGoalsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Goal = new ApiPerformance.Goal
                {
                    Comments = { new ApiPerformance.GoalComment
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc0 = _performanceGoalValidationService.LintGoalsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiPerformance.PreviewCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Status = (request.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "PreviewCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ScoreCalibrationResponse> ScoreCalibration(ApiPerformance.ScoreCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.EstimateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.EstimateFeedbackRequest.Types.Details
            {
                Budget = (request.Details?.Budget ?? new ApiCommon.Money()),
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _performanceFeedbackCalculationService.EstimateFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Calibration?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.CalibrationComment>()).Select(async item =>
        {
            var input1 = new ApiWorkplaces.RecalculateVisitorPassesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                VisitorPass = new ApiWorkplaces.VisitorPass
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _workplacesVisitorPassCalculationService.RecalculateVisitorPassesAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiWorkplaces.RecalculateVisitorPassesResponse();
        var response = new ApiPerformance.ScoreCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Subject = (request.Calibration?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ScoreCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ValidateCalibrationResponse> ValidateCalibration(ApiPerformance.ValidateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.QuoteFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                    {
                        ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Calibration?.Requester?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _performanceFeedbackCalculationService.QuoteFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ValidateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
                Subject = (request.Calibration?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ValidateCalibration", (request.Details?.Locale ?? ""), token);
        return response;
    }
    private async Task<ApiPerformance.EstimateCalibrationResponse> SendCheckCalibrationEligibility1Async(ApiPerformance.EstimateCalibrationRequest request, CancellationToken token)
    {
        return await _calibrationValidationEndpointSendCheckCalibrationEligibility1Port.SendAsync(request, token);
    }
}
