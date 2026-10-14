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
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class RoomBookingValidationEndpoint
{
    /// <summary>
    /// Checks a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.CheckRoomBookingEligibilityResponse> CheckRoomBookingEligibility(ApiWorkplaces.CheckRoomBookingEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckRoomBookingEligibility for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.NormalizeSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.NormalizeSlaPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.RoomBooking?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _helpdeskSlaPolicyValidationService.NormalizeSlaPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.BatchCreateSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiSearch.SearchIndex
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var stream1 = _searchSearchIndexService.BatchCreateSearchIndexes(cancellationToken: token);
        await stream1.RequestStream.WriteAsync(input1, token);
        await stream1.RequestStream.CompleteAsync();
        var reply1 = await stream1.ResponseAsync;
        var input2 = new ApiWorkplaces.CheckDeskEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.RoomBooking?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await SendCheckRoomBookingEligibility2Async(input2, token);
        var response = new ApiWorkplaces.CheckRoomBookingEligibilityResponse
        {
            Outcome = new ApiWorkplaces.CheckRoomBookingEligibilityResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CheckRoomBookingEligibility", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainRoomBookingDecisionResponse> ExplainRoomBookingDecision(ApiWorkplaces.ExplainRoomBookingDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRoomBookingDecision for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.QuoteTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.RoomBooking?.Location?.CountryCode ?? ""),
                    PostalCode = (request.RoomBooking?.Location?.PostalCode ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.RoomBooking?.Audit?.CreatedBy ?? ""),
                    Etag = (request.RoomBooking?.Audit?.Etag ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskTicketCalculationService.QuoteTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ExplainRoomBookingDecisionResponse
        {
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Ticket?.Assignee?.Email ?? ""),
                    },
                } },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ExplainRoomBookingDecision", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.GetRoomBookingRulesResponse> GetRoomBookingRules(ApiWorkplaces.GetRoomBookingRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        _logger.LogInformation("Handling GetRoomBookingRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "GetRoomBookingRules", "accepted", token);
        var input0 = new ApiIdentity.ExportUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.ExportUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.RoomBookingId ?? ""),
            },
        };
        using var stream0 = _identityUserQueryService.ExportUsers(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportUsers frame {Type}", change0.GetType().Name);
        }
        var response = new ApiWorkplaces.GetRoomBookingRulesResponse
        {
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.LintRoomBookingsResponse> LintRoomBookings(ApiWorkplaces.LintRoomBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintRoomBookings for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.LintChangeRequestsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _itsmChangeRequestValidationService.LintChangeRequestsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CheckBuildingEligibilityRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                            UpdatedBy = (request.RoomBooking?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.RoomBooking?.Location?.Locality ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _workplacesBuildingValidationService.CheckBuildingEligibilityAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiWorkplaces.ScoreBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply1.Building?.Audit?.CreatedBy ?? ""),
                            Etag = (reply1.Building?.Audit?.Etag ?? ""),
                            UpdatedBy = (reply1.Building?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.RoomBooking?.Location?.PostalCode ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _workplacesBuildingValidationService.ScoreBuildingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()).Select(async item =>
        {
            var input3 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.SimulateKnowledgeArticleRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply1.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply1.Building?.Audit?.UpdatedBy ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc3 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiHelpdesk.SimulateKnowledgeArticleResponse();
        var response = new ApiWorkplaces.LintRoomBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Building?.Audit?.Etag ?? ""),
                },
                Location = (reply2.Building?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Building?.Manager?.DisplayName ?? ""),
                    Email = (reply2.Building?.Manager?.Email ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "LintRoomBookings", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.NormalizeRoomBookingResponse> NormalizeRoomBooking(ApiWorkplaces.NormalizeRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckDocumentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.NormalizeRoomBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Document?.BillingAddress?.PostalCode ?? ""),
                },
                Organizer = (request.RoomBooking?.Organizer ?? new ApiIdentity.UserRef()),
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "NormalizeRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.PreviewRoomBookingResponse> PreviewRoomBooking(ApiWorkplaces.PreviewRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.RoomBooking?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync);
        var input1 = new ApiWorkplaces.ExplainBuildingDecisionRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.RoomBooking?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.RoomBooking?.Organizer?.DisplayName ?? ""),
                            Email = (request.Details?.Contact?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendPreviewRoomBooking1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.RecalculateRoomBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.RoomBooking?.Location?.PostalCode ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                        PreferredChannel = (reply0.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var named2 = _registry.Get<ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient>("acme.workplaces.v1.RoomBookingCalculationService");
        using var rpc2 = named2.RecalculateRoomBookingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.PreviewRoomBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                Location = (request.RoomBooking?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Building?.Manager?.Email ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "PreviewRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ScoreRoomBookingResponse> ScoreRoomBooking(ApiWorkplaces.ScoreRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiNotifications.ValidateNotificationResponse();
        foreach (var item in (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiNotifications.ValidateNotificationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiNotifications.ValidateNotificationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
                Notification = new ApiNotifications.Notification
                {
                    Requester = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc0 = _notificationsNotificationValidationService.ValidateNotificationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiWorkplaces.RecalculateVisitorPassesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.RecalculateVisitorPassesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiWorkplaces.RecalculateVisitorPassesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _workplacesVisitorPassCalculationService.RecalculateVisitorPassesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiWorkplaces.ScoreRoomBookingRequest, ApiIdentity.LookupUserRequest>(value => new ApiIdentity.LookupUserRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            UserId = (value.RoomBooking?.Organizer?.UserId ?? ""),
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _identityUserQueryService.LookupUserAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ScoreRoomBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Organizer = (request.RoomBooking?.Organizer ?? new ApiIdentity.UserRef()),
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (reply2.User?.Contact?.Phone ?? ""),
                    },
                } },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ScoreRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ValidateRoomBookingResponse> ValidateRoomBooking(ApiWorkplaces.ValidateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiWorkplaces.ValidateBuildingResponse();
        foreach (var item in (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiWorkplaces.ValidateBuildingRequest
            {
                Building = new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Manager = new ApiIdentity.UserRef
                            {
                                Email = (item.Contact?.Email ?? ""),
                            },
                        } },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiWorkplaces.ValidateBuildingRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc0 = _workplacesBuildingValidationService.ValidateBuildingAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = request.ValidateRoomBookingToEstimateVisitorPass1(reply0);
        using var rpc1 = _workplacesVisitorPassCalculationService.EstimateVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.LintDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.VisitorPass?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply2 = await SendValidateRoomBooking2Async(input2, token);
        var response = new ApiWorkplaces.ValidateRoomBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.VisitorPass?.BillingAddress?.Locality ?? ""),
                    PostalCode = (reply2.Desk?.Address?.PostalCode ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Window = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ValidateRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.CheckDeskEligibilityResponse> SendCheckRoomBookingEligibility2Async(ApiWorkplaces.CheckDeskEligibilityRequest request, CancellationToken token)
    {
        return await _roomBookingValidationEndpointSendCheckRoomBookingEligibility2Port.SendAsync(request, token);
    }
    private async Task<ApiWorkplaces.LintDesksResponse> SendValidateRoomBooking2Async(ApiWorkplaces.LintDesksRequest request, CancellationToken token)
    {
        return await _roomBookingValidationEndpointSendValidateRoomBooking2Port.SendAsync(request, token);
    }
}
