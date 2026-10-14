namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationSegmentViewChecks
{
    public static void AttestationSegmentViewShowsTheStatusInCapitals()
    {
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
        var view = AttestationSegmentView.ToAttestationSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationSegmentView.ToAttestationSegmentViews(new List<AttestationSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationSegmentViewShowsTheStatusInCapitals();
    }
}
