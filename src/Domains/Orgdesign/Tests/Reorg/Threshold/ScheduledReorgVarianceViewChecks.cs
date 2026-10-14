namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledReorgVarianceViewChecks
{
    public static void ScheduledReorgVarianceViewShowsTheStatusInCapitals()
    {
        var item = ScheduledReorgVariance.NewScheduledReorgVariance("code-363", "label-857");
        var view = ScheduledReorgVarianceView.ToScheduledReorgVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledReorgVarianceView.ToScheduledReorgVarianceViews(new List<ScheduledReorgVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledReorgVarianceViewShowsTheStatusInCapitals();
    }
}
