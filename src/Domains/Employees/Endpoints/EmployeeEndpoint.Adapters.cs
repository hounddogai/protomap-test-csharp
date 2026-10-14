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
using ApiExperiments = Acme.Experiments.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public static class EmployeeEndpointRequests
{
    /// <summary>
    /// Builds the BrowseOrgUnits request from the current workflow state.
    /// </summary>
    public static ApiEmployees.BrowseOrgUnitsRequest WatchEmployeesToBrowseOrgUnits2(this ApiEmployees.WatchEmployeesRequest request, ApiExperiments.CloneExperimentResponse reply0)
    {
        return new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (reply0.Experiment?.Location?.Region ?? ""),
            },
        };
    }
}
