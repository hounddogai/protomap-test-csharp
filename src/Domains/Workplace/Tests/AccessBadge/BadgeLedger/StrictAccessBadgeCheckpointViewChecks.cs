namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class StrictAccessBadgeCheckpointViewChecks
{
    public static void StrictAccessBadgeCheckpointViewShowsTheStatusInCapitals()
    {
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("code-11", "name-889");
        var view = StrictAccessBadgeCheckpointView.ToStrictAccessBadgeCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAccessBadgeCheckpointView.ToStrictAccessBadgeCheckpointViews(new List<StrictAccessBadgeCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAccessBadgeCheckpointViewShowsTheStatusInCapitals();
    }
}
