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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Workplaces;

public static class DeskLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the CloneYard request from the current workflow state.
    /// </summary>
    public static ApiYards.CloneYardRequest TerminateDeskToCloneYard1(this ApiWorkplaces.TerminateDeskRequest request)
    {
        return new ApiYards.CloneYardRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Yard = new ApiYards.Yard
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
    }
}
