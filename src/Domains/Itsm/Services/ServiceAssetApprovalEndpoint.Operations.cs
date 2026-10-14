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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ServiceAssetApprovalEndpoint
{
    /// <summary>
    /// Approves a service asset.
    /// </summary>
    public override async Task<ApiItsm.ApproveServiceAssetResponse> ApproveServiceAsset(ApiItsm.ApproveServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SimulateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Quantity = (request.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentFolderVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                            {
                                ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                                _ => new Google.Protobuf.Collections.RepeatedField<string>()
                            } },
                            Recipient = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                            {
                                ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Recipient ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.CompareDocumentFolderVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ApproveServiceAssetResponse
        {
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.DocumentFolder?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ApproveServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a service asset.
    /// </summary>
    public override async Task<ApiItsm.CommentServiceAssetApprovalResponse> CommentServiceAssetApproval(ApiItsm.CommentServiceAssetApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentServiceAssetApproval for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.PushProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PushProblemRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.PushProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScheduleProblemReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ProblemId = (reply0.Problem?.ProblemId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _itsmProblemReportingService.ScheduleProblemReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CommentServiceAssetApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CommentServiceAssetApproval", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a service asset.
    /// </summary>
    public override async Task<ApiItsm.DelegateServiceAssetApprovalResponse> DelegateServiceAssetApproval(ApiItsm.DelegateServiceAssetApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateServiceAssetApproval for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.PullProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PullProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.ServiceAsset?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _itsmProblemSyncService.PullProblemsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ApproveIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    UserId = (reply0.Problem?.Assignee?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiItsm.ApproveIncidentRequest>(json1);
        using var rpc1 = _itsmIncidentApprovalService.ApproveIncidentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.SignalManagedDeviceWorkflowRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiDevices.SignalManagedDeviceWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (reply1.Incident?.Assignee?.Email ?? ""),
            },
        };
        input2.ManagedDevice = new ApiDevices.ManagedDevice
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (reply0.Problem?.Audit?.Etag ?? ""),
            },
            Dimensions = new ApiDevices.ManagedDeviceDimensions
            {
                Length = (request.ServiceAsset?.Dimensions?.Length ?? 0d),
            },
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
            },
        };
        using var rpc2 = _devicesManagedDeviceWorkflowService.SignalManagedDeviceWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.DelegateServiceAssetApprovalResponse
        {
            Outcome = new ApiItsm.DelegateServiceAssetApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Problem?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply2.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Problem?.Assignee?.DisplayName ?? ""),
                    Email = (reply2.ManagedDevice?.Owner?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "DelegateServiceAssetApproval", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a service asset.
    /// </summary>
    public override async Task<ApiItsm.EscalateServiceAssetResponse> EscalateServiceAsset(ApiItsm.EscalateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ScheduleChangeRequestReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleChangeRequestReportRequest.Types.Details
            {
                Filters = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _itsmChangeRequestReportingService.ScheduleChangeRequestReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiMaintenance.BatchGetWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _maintenanceWorkOrderService.BatchGetWorkOrdersAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.EscalateServiceAssetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Code = (request.ServiceAsset?.Code ?? ""),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "EscalateServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists service assets a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListPendingServiceAssetsResponse> ListPendingServiceAssets(ApiItsm.ListPendingServiceAssetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingServiceAssets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", "batch",
            "ListPendingServiceAssets", "accepted", token);
        var input0 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CalculateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync);
        var response = new ApiItsm.ListPendingServiceAssetsResponse
        {
            ServiceAssets = { new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ListPendingServiceAssets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a service asset.
    /// </summary>
    public override async Task<ApiItsm.RejectServiceAssetResponse> RejectServiceAsset(ApiItsm.RejectServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Dimensions = new ApiItsm.ServiceAssetDimensions
                {
                    Length = (request.ServiceAsset?.Dimensions?.Length ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendRejectServiceAsset0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ResolveProblemConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ResolveProblemConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ServiceAsset?.Audit?.Etag ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmProblemSyncService.ResolveProblemConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor2 = new Method<ApiFeatureflags.NormalizeFeatureFlagRequest, ApiFeatureflags.NormalizeFeatureFlagResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagValidationService", "NormalizeFeatureFlag",
            Wire.Marshaller<ApiFeatureflags.NormalizeFeatureFlagRequest>(), Wire.Marshaller<ApiFeatureflags.NormalizeFeatureFlagResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.RejectServiceAssetResponse
        {
            Outcome = new ApiItsm.RejectServiceAssetResponse.Types.Outcome
            {
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Problem?.Assignee?.DisplayName ?? ""),
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "RejectServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a service asset.
    /// </summary>
    public override async Task<ApiItsm.SubmitServiceAssetResponse> SubmitServiceAsset(ApiItsm.SubmitServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ApproveSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ApproveSoftwareLicenseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.ApproveSoftwareLicenseRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingSoftwareLicenseApprovalService.ApproveSoftwareLicenseAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.SubmitServiceAssetRequest, ApiItsm.AdvanceIncidentWorkflowRequest>(value => new ApiItsm.AdvanceIncidentWorkflowRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (value.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmIncidentWorkflowService.AdvanceIncidentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.SubmitServiceAssetResponse
        {
            Outcome = new ApiItsm.SubmitServiceAssetResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "SubmitServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
}
