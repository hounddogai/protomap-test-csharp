namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalExceptionCheckpointViewChecks
{
    public static void RegionalExceptionCheckpointViewShowsTheStatusInCapitals()
    {
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
        var view = RegionalExceptionCheckpointView.ToRegionalExceptionCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalExceptionCheckpointView.ToRegionalExceptionCheckpointViews(new List<RegionalExceptionCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalExceptionCheckpointViewShowsTheStatusInCapitals();
    }
}
