namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotViewChecks
{
    public static void GlobalNominationSnapshotViewShowsTheStatusInCapitals()
    {
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
        var view = GlobalNominationSnapshotView.ToGlobalNominationSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalNominationSnapshotView.ToGlobalNominationSnapshotViews(new List<GlobalNominationSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalNominationSnapshotViewShowsTheStatusInCapitals();
    }
}
