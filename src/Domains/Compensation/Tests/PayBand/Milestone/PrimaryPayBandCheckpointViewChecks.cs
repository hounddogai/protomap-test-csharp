namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PrimaryPayBandCheckpointViewChecks
{
    public static void PrimaryPayBandCheckpointViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
        var view = PrimaryPayBandCheckpointView.ToPrimaryPayBandCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPayBandCheckpointView.ToPrimaryPayBandCheckpointViews(new List<PrimaryPayBandCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPayBandCheckpointViewShowsTheStatusInCapitals();
    }
}
