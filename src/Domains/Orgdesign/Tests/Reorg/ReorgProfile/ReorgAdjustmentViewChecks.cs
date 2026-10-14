namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentViewChecks
{
    public static void ReorgAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
        var view = ReorgAdjustmentView.ToReorgAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgAdjustmentView.ToReorgAdjustmentViews(new List<ReorgAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgAdjustmentViewShowsTheStatusInCapitals();
    }
}
