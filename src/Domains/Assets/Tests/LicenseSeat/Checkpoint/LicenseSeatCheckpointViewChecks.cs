namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatCheckpointViewChecks
{
    public static void LicenseSeatCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("code-647", "title-43");
        var view = LicenseSeatCheckpointView.ToLicenseSeatCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatCheckpointView.ToLicenseSeatCheckpointViews(new List<LicenseSeatCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatCheckpointViewShowsTheStatusInCapitals();
    }
}
