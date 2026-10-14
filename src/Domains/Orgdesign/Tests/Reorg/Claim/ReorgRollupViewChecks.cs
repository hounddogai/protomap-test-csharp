namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupViewChecks
{
    public static void ReorgRollupViewShowsTheStatusInCapitals()
    {
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
        var view = ReorgRollupView.ToReorgRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgRollupView.ToReorgRollupViews(new List<ReorgRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgRollupViewShowsTheStatusInCapitals();
    }
}
