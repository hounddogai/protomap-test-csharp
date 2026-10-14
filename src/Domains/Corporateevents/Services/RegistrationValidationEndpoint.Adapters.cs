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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public static class RegistrationValidationEndpointRequests
{
    /// <summary>
    /// Builds the QuoteCorporateEvent request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.QuoteCorporateEventRequest NormalizeRegistrationToQuoteCorporateEvent2(this ApiCorporateevents.NormalizeRegistrationRequest request, ApiCorporateevents.EstimateRegistrationResponse reply0)
    {
        return new ApiCorporateevents.QuoteCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
    }
}
