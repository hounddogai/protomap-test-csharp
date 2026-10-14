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
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class CandidateWorkflowEndpoint
{
    /// <summary>
    /// Advances a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.AdvanceCandidateWorkflowResponse> AdvanceCandidateWorkflow(ApiRecruiting.AdvanceCandidateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceCandidateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiVenues.UpdateVenueRequest, ApiVenues.UpdateVenueResponse>(
            MethodType.Unary, "acme.venues.v1" + ".VenueService", "UpdateVenue",
            Wire.Marshaller<ApiVenues.UpdateVenueRequest>(), Wire.Marshaller<ApiVenues.UpdateVenueResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SuspendCandidateRequest
        {
            CandidateId = (request.Candidate?.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SuspendCandidateRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Candidate?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.SuspendCandidateRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingCandidateLifecycleService.SuspendCandidateAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.AdvanceCandidateWorkflowResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CandidateId = (request.CandidateId ?? ""),
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Candidate?.Contact?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Venue?.Address?.Region ?? ""),
                },
                Status = (request.Candidate?.Status ?? (ApiRecruiting.CandidateStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "AdvanceCandidateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.CancelCandidateWorkflowResponse> CancelCandidateWorkflow(ApiRecruiting.CancelCandidateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelCandidateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiRecruiting.CancelCandidateWorkflowRequest, ApiRecruiting.ListInterviewConflictsRequest>(value => new ApiRecruiting.ListInterviewConflictsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ListInterviewConflictsRequest.Types.Filter
            {
                CreatedAfter = (value.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (value.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingInterviewSyncService.ListInterviewConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RecordSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
            {
                AuthorEmail = (request.Candidate?.Contact?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    Number = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.NationalId => (request.Candidate?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SubmitCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Candidate?.Contact?.Phone ?? ""),
                },
                NationalId = new ApiRecruiting.CandidateNationalId
                {
                    IssuingCountry = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.NationalId => (request.Candidate?.NationalId?.IssuingCountry ?? ""),
                        _ => ""
                    },
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _recruitingCandidateApprovalService.SubmitCandidateAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.CancelCandidateWorkflowResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Attributes = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
            },
            Outcome = new ApiRecruiting.CancelCandidateWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CancelCandidateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListCandidateWorkflowsResponse> ListCandidateWorkflows(ApiRecruiting.ListCandidateWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListCandidateWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "ListCandidateWorkflows", "accepted", token);
        var input0 = new ApiRecruiting.AdvanceJobOpeningWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.AdvanceJobOpeningWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await SendListCandidateWorkflows0Async(input0, token);
        var response = new ApiRecruiting.ListCandidateWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ListCandidateWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.PauseCandidateWorkflowResponse> PauseCandidateWorkflow(ApiRecruiting.PauseCandidateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseCandidateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.GetCandidateApprovalStatusRequest
        {
            CandidateId = (request.Candidate?.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingCandidateApprovalService.GetCandidateApprovalStatusAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Candidate?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiEmployees.EstimateEmploymentContractRequest>();
        using var rpc1 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.PauseCandidateWorkflowResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                Name = (request.Candidate?.Name ?? new ApiCommon.PersonName()),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Candidate?.Contact?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "PauseCandidateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.SignalCandidateWorkflowResponse> SignalCandidateWorkflow(ApiRecruiting.SignalCandidateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalCandidateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ResumeOvertimeWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTimekeeping.ResumeOvertimeWorkflowRequest>(json0);
        using var rpc0 = _timekeepingOvertimeWorkflowService.ResumeOvertimeWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.BatchGetEmployeesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc1 = _employeesEmployeeService.BatchGetEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.FindUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.FindUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                {
                    ApiRecruiting.Candidate.IdentityDocumentOneofCase.Passport => (request.Candidate?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Region = (request.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _identityUserQueryService.FindUsersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.SignalCandidateWorkflowResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = (request.Candidate?.PrimaryAddress ?? new ApiCommon.PostalAddress()),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Overtime?.Owner?.Email ?? ""),
                },
            },
            Outcome = new ApiRecruiting.SignalCandidateWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SignalCandidateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.StartCandidateWorkflowResponse> StartCandidateWorkflow(ApiRecruiting.StartCandidateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartCandidateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Candidate?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.LintPositionsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiIdentity.SuggestUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter
            {
                Region = (request.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _identityUserQueryService.SuggestUsersAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.RejectCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _recruitingCandidateApprovalService.RejectCandidateAsync);
        var input3 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _sendStartCandidateWorkflow3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.StartCandidateWorkflowResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply2.Candidate?.Audit?.Etag ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Candidate?.RelationshipManager?.Email ?? ""),
                },
                Name = (request.Candidate?.Name ?? new ApiCommon.PersonName()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "StartCandidateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.AdvanceJobOpeningWorkflowResponse> SendListCandidateWorkflows0Async(ApiRecruiting.AdvanceJobOpeningWorkflowRequest request, CancellationToken token)
    {
        return await _candidateWorkflowEndpointSendListCandidateWorkflows0Port.SendAsync(request, token);
    }
}
