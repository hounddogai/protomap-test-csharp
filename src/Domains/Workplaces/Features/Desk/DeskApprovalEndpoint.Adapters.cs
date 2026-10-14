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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public static class DeskApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ExplainDeskCalculation request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.ExplainDeskCalculationRequest CommentDeskApprovalToExplainDeskCalculation2(this ApiWorkplaces.CommentDeskApprovalRequest request)
    {
        return new ApiWorkplaces.ExplainDeskCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Desk?.Audit?.UpdatedBy ?? ""),
                },
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Width = (request.Desk?.Dimensions?.Width ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
    }
}
