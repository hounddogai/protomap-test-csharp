namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class PendingPoolTargetViewChecks
{
    public static void PendingPoolTargetViewShowsTheStatusInCapitals()
    {
        var item = PendingPoolTarget.NewPendingPoolTarget("code-882", "label-191");
        var view = PendingPoolTargetView.ToPendingPoolTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPoolTargetView.ToPendingPoolTargetViews(new List<PendingPoolTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPoolTargetViewShowsTheStatusInCapitals();
    }
}
