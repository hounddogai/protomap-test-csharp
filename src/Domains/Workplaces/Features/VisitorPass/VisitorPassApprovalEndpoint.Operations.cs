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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class VisitorPassApprovalEndpoint
{
    /// <summary>
    /// Approves a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ApproveVisitorPassResponse> ApproveVisitorPass(ApiWorkplaces.ApproveVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CompareRoomBookingOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.CompareRoomBookingOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.VisitorPass?.BillingAddress?.PostalCode ?? ""),
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await SendApproveVisitorPass0Async(input0, token);
        var input1 = new ApiDocuments.WatchDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.WatchDocumentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.RoomBooking?.Location?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
            },
        };
        using var stream1 = _documentsDocumentService.WatchDocuments(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDocuments frame {Type}", change1.GetType().Name);
        }
        var response = new ApiWorkplaces.ApproveVisitorPassResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.VisitorPass?.IssuedAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ApproveVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.CommentVisitorPassApprovalResponse> CommentVisitorPassApproval(ApiWorkplaces.CommentVisitorPassApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentVisitorPassApproval for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPipelines.RecalculatePipelinesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPipelines.RecalculatePipelinesRequest>();
        using var rpc0 = _pipelinesPipelineCalculationService.RecalculatePipelinesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.CheckPositionEligibilityRequest>(json1);
        using var rpc1 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CommentVisitorPassApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                DueAt = (request.VisitorPass?.DueAt ?? new Timestamp()),
                Metadata = { (reply0.Pipeline?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "CommentVisitorPassApproval", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.DelegateVisitorPassApprovalResponse> DelegateVisitorPassApproval(ApiWorkplaces.DelegateVisitorPassApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateVisitorPassApproval for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ShareVisitorPassReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ShareVisitorPassReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
            VisitorPassId = (request.VisitorPass?.VisitorPassId ?? ""),
        };
        using var rpc0 = _workplacesVisitorPassReportingService.ShareVisitorPassReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.GetUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Details?.Approver?.UserId ?? ""),
        };
        using var rpc1 = _identityUserService.GetUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiIdentity.GetUserRequest>("acme.workplaces.v1.VisitorPassApprovalService/DelegateVisitorPassApproval/1", input1.Clone()), token);
        var response = new ApiWorkplaces.DelegateVisitorPassApprovalResponse
        {
            Outcome = new ApiWorkplaces.DelegateVisitorPassApprovalResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.User?.RelationshipManager?.DisplayName ?? ""),
                },
                DueAt = (request.VisitorPass?.DueAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "DelegateVisitorPassApproval", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GetVisitorPassApprovalStatusResponse> GetVisitorPassApprovalStatus(ApiWorkplaces.GetVisitorPassApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        _logger.LogInformation("Handling GetVisitorPassApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GetVisitorPassApprovalStatus", "accepted", token);
        var input0 = new ApiBoxoffice.UpdateBoxOfficeSaleRequest
        {
            BoxOfficeSale = new ApiBoxoffice.BoxOfficeSale
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiBoxoffice.BoxOfficeSaleLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _boxofficeBoxOfficeSaleService.UpdateBoxOfficeSaleAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.GetVisitorPassApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists visitor passes a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListPendingVisitorPassesResponse> ListPendingVisitorPasses(ApiWorkplaces.ListPendingVisitorPassesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingVisitorPasses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", "batch",
            "ListPendingVisitorPasses", "accepted", token);
        var input0 = new ApiWorkplaces.AdvanceBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Filter?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.AdvanceBuildingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesBuildingWorkflowService.AdvanceBuildingWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _employeesEmployeeService.SearchEmployeesAsync);
        var response = new ApiWorkplaces.ListPendingVisitorPassesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            VisitorPasses = { new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Building?.Location?.Locality ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ListPendingVisitorPasses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.RecallVisitorPassResponse> RecallVisitorPass(ApiWorkplaces.RecallVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendRecallVisitorPass0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.RecallVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.RecallVisitorPassResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                DueAt = (request.VisitorPass?.DueAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "RecallVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.SubmitVisitorPassResponse> SubmitVisitorPass(ApiWorkplaces.SubmitVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.VisitorPass?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.VisitorPass?.BillingAddress?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.VisitorPass?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc0 = named0.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiEmployees.NormalizeEmploymentContractResponse();
        foreach (var item in (request.VisitorPass?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.VisitorPassLine>()))
        {
            var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Lines = { new ApiEmployees.EmploymentContractLine
                    {
                        LineId = (item.LineId ?? ""),
                        Tax = new ApiEmployees.EmploymentContractLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc1 = _employeesEmploymentContractValidationService.NormalizeEmploymentContractAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var reply2 = new ApiEmployees.SearchEmployeesResponse();
        foreach (var item in (request.VisitorPass?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.VisitorPassLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiEmployees.SearchEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
                {
                    OwnerId = (item.LineId ?? ""),
                    Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc2 = _employeesEmployeeService.SearchEmployeesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var profile3 = new MappingProfile<ApiWorkplaces.SubmitVisitorPassRequest, ApiDocuments.ListDocumentTemplateVersionsRequest>(value => new ApiDocuments.ListDocumentTemplateVersionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
            {
                CreatedAfter = (reply1.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.VisitorPass?.VisitorPassId ?? ""),
                Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.SubmitVisitorPassResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Number = (request.VisitorPass?.Number ?? ""),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "SubmitVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.CompareRoomBookingOptionsResponse> SendApproveVisitorPass0Async(ApiWorkplaces.CompareRoomBookingOptionsRequest request, CancellationToken token)
    {
        return await _visitorPassApprovalEndpointSendApproveVisitorPass0Port.SendAsync(request, token);
    }
}
