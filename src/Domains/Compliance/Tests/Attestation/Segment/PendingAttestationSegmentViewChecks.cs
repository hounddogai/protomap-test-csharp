namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PendingAttestationSegmentViewChecks
{
    public static void PendingAttestationSegmentViewShowsTheStatusInCapitals()
    {
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
        var view = PendingAttestationSegmentView.ToPendingAttestationSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAttestationSegmentView.ToPendingAttestationSegmentViews(new List<PendingAttestationSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAttestationSegmentViewShowsTheStatusInCapitals();
    }
}
