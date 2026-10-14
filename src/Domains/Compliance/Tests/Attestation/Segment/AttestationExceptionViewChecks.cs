namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AttestationExceptionViewChecks
{
    public static void AttestationExceptionViewShowsTheStatusInCapitals()
    {
        var item = AttestationException.NewAttestationException("code-467", "title-812");
        var view = AttestationExceptionView.ToAttestationExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationExceptionView.ToAttestationExceptionViews(new List<AttestationException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationExceptionViewShowsTheStatusInCapitals();
    }
}
