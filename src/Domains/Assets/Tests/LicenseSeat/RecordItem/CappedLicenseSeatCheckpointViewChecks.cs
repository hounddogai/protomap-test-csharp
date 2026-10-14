namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedLicenseSeatCheckpointViewChecks
{
    public static void CappedLicenseSeatCheckpointViewShowsTheStatusInCapitals()
    {
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("code-948", "title-530");
        var view = CappedLicenseSeatCheckpointView.ToCappedLicenseSeatCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedLicenseSeatCheckpointView.ToCappedLicenseSeatCheckpointViews(new List<CappedLicenseSeatCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedLicenseSeatCheckpointViewShowsTheStatusInCapitals();
    }
}
