namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgThresholdViewChecks
{
    public static void ReorgThresholdViewShowsTheStatusInCapitals()
    {
        var item = ReorgThreshold.NewReorgThreshold("code-85", "title-901");
        var view = ReorgThresholdView.ToReorgThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgThresholdView.ToReorgThresholdViews(new List<ReorgThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgThresholdViewShowsTheStatusInCapitals();
    }
}
