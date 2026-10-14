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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ProblemValidationEndpoint
{
    /// <summary>
    /// Checks a problem.
    /// </summary>
    public override async Task<ApiItsm.CheckProblemEligibilityResponse> CheckProblemEligibility(ApiItsm.CheckProblemEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckProblemEligibility for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ValidateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Problem?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Problem?.Audit?.UpdatedBy ?? ""),
                },
                Sla = new ApiItsm.ChangeRequestSla
                {
                    DueAt = (request.Problem?.Sla?.DueAt ?? new Timestamp()),
                    PolicyId = (request.Problem?.Sla?.PolicyId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmChangeRequestValidationService.ValidateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.CheckProblemEligibilityToResolveSoftwareLicenses1();
        using var rpc1 = _licensingSoftwareLicenseQueryService.ResolveSoftwareLicensesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CheckProblemEligibilityResponse
        {
            Outcome = new ApiItsm.CheckProblemEligibilityResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Subject = (request.Problem?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "CheckProblemEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a problem.
    /// </summary>
    public override async Task<ApiItsm.ExplainProblemDecisionResponse> ExplainProblemDecision(ApiItsm.ExplainProblemDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainProblemDecision for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.NormalizeAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventValidationService.NormalizeAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Problem?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemStatusChange>()).Select(async item =>
        {
            var input1 = new ApiItsm.QuoteProblemRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Problem = new ApiItsm.Problem
                {
                    History = { new ApiItsm.ProblemStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                        FromStatus = (item.FromStatus ?? ""),
                        ToStatus = (item.ToStatus ?? ""),
                    } },
                },
            };
            using var rpc1 = _itsmProblemCalculationService.QuoteProblemAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiItsm.QuoteProblemResponse();
        var response = new ApiItsm.ExplainProblemDecisionResponse
        {
            Outcome = new ApiItsm.ExplainProblemDecisionResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ExplainProblemDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.GetProblemRulesResponse> GetProblemRules(ApiItsm.GetProblemRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling GetProblemRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GetProblemRules", "accepted", token);
        var input0 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ScheduleDocumentExportRequest>();
        using var rpc0 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetProblemRulesResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a problem.
    /// </summary>
    public override async Task<ApiItsm.LintProblemsResponse> LintProblems(ApiItsm.LintProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintProblems for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CountSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.Problem?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Problem?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Problem?.Sla?.PolicyId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.CountSoftwareLicensesRequest>(json0);
        using var rpc0 = _licensingSoftwareLicenseQueryService.CountSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(async item =>
        {
            var input1 = new ApiItsm.CalculateProblemRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Problem = new ApiItsm.Problem
                {
                    Comments = { new ApiItsm.ProblemComment
                    {
                        Body = (item.Body ?? ""),
                    } },
                },
            };
            using var rpc1 = _itsmProblemCalculationService.CalculateProblemAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiItsm.CalculateProblemResponse();
        var input2 = new ApiItsm.SimulateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Problem?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
                Sla = new ApiItsm.ProblemSla
                {
                    DueAt = (request.Problem?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _itsmProblemCalculationService.SimulateProblemAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiIdentity.ValidateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ValidateUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.Problem?.Requester?.Phone ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _identityUserValidationService.ValidateUserAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.LintProblemsResponse
        {
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply3.User?.UserId ?? ""),
                },
                CustomFields = { (request.Problem?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ProblemId = (request.ProblemId ?? ""),
                Sla = new ApiItsm.ProblemSla
                {
                    Breached = (reply2.Problem?.Sla?.Breached ?? false),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "LintProblems", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a problem.
    /// </summary>
    public override async Task<ApiItsm.PreviewProblemResponse> PreviewProblem(ApiItsm.PreviewProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentFolderRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.Problem?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (request.Problem?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Problem?.Assignee?.DisplayName ?? ""),
                            Email = (request.Problem?.Requester?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PushDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiMetrics.ExplainMetricSeriesDecisionResponse();
        foreach (var item in (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()))
        {
            var input1 = new ApiMetrics.ExplainMetricSeriesDecisionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                MetricSeries = new ApiMetrics.MetricSeries
                {
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
            using var rpc1 = _metricsMetricSeriesValidationService.ExplainMetricSeriesDecisionAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiItsm.QuoteServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.MetricSeries?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _sendPreviewProblem2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(async item =>
        {
            var input3 = new ApiItsm.SimulateProblemRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Problem = new ApiItsm.Problem
                {
                    Comments = { new ApiItsm.ProblemComment
                    {
                        CommentId = (item.CommentId ?? ""),
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc3 = _itsmProblemCalculationService.SimulateProblemAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiItsm.SimulateProblemResponse();
        var response = new ApiItsm.PreviewProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "PreviewProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a problem.
    /// </summary>
    public override async Task<ApiItsm.ScoreProblemResponse> ScoreProblem(ApiItsm.ScoreProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiSearch.ValidateSearchIndexResponse();
        foreach (var item in (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()))
        {
            var input0 = new ApiSearch.ValidateSearchIndexRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiSearch.ValidateSearchIndexRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
                SearchIndex = new ApiSearch.SearchIndex
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc0 = _searchSearchIndexValidationService.ValidateSearchIndexAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiItsm.CalculateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                Sla = new ApiItsm.ChangeRequestSla
                {
                    Breached = (request.Problem?.Sla?.Breached ?? false),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CalculateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                    {
                        ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _itsmChangeRequestCalculationService.CalculateChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ScoreProblemResponse
        {
            Breakdown = new ApiItsm.ScoreProblemResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Priority = (reply1.ChangeRequest?.Priority ?? 0),
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ScoreProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a problem.
    /// </summary>
    public override async Task<ApiItsm.ValidateProblemResponse> ValidateProblem(ApiItsm.ValidateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Problem?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemStatusChange>()).Select(async item =>
        {
            var input0 = new ApiItsm.NormalizeServiceAssetRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.NormalizeServiceAssetRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Problem?.Assignee?.Email ?? ""),
                        Name = (request.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                ServiceAsset = new ApiItsm.ServiceAsset
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.ChangedAt ?? new Timestamp()),
                        Etag = (request.Problem?.Audit?.Etag ?? ""),
                    },
                },
            };
            using var rpc0 = _itsmServiceAssetValidationService.NormalizeServiceAssetAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiItsm.NormalizeServiceAssetResponse();
        var response = new ApiItsm.ValidateProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Subject = (request.Problem?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ValidateProblem", (request.Reason ?? ""), token);
        return response;
    }
}
