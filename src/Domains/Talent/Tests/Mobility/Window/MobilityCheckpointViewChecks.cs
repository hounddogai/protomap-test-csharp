namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityCheckpointViewChecks
{
    public static void MobilityCheckpointViewShowsTheStatusInCapitals()
    {
        var item = MobilityCheckpoint.NewMobilityCheckpoint("code-150", "name-773");
        var view = MobilityCheckpointView.ToMobilityCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityCheckpointView.ToMobilityCheckpointViews(new List<MobilityCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityCheckpointViewShowsTheStatusInCapitals();
    }
}
