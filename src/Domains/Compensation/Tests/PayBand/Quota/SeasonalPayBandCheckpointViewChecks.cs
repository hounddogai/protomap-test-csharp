namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalPayBandCheckpointViewChecks
{
    public static void SeasonalPayBandCheckpointViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("code-962", "label-379");
        var view = SeasonalPayBandCheckpointView.ToSeasonalPayBandCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPayBandCheckpointView.ToSeasonalPayBandCheckpointViews(new List<SeasonalPayBandCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPayBandCheckpointViewShowsTheStatusInCapitals();
    }
}
