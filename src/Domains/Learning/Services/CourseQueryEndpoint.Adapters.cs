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
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for ValidateDocument.
/// </summary>
public interface ICourseQueryEndpointSendCountCourses0Port
{
    Task<ApiDocuments.ValidateDocumentResponse> SendAsync(ApiDocuments.ValidateDocumentRequest request, CancellationToken token);
}

public sealed class CourseQueryEndpointSendCountCourses0PortAdapter(ApiDocuments.DocumentValidationService.DocumentValidationServiceClient client) : ICourseQueryEndpointSendCountCourses0Port
{
    public async Task<ApiDocuments.ValidateDocumentResponse> SendAsync(ApiDocuments.ValidateDocumentRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateDocumentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
