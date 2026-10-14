namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandMilestoneViewChecks
{
    public static void PayBandMilestoneViewShowsTheStatusInCapitals()
    {
        var item = PayBandMilestone.NewPayBandMilestone("code-715", "title-723");
        var view = PayBandMilestoneView.ToPayBandMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandMilestoneView.ToPayBandMilestoneViews(new List<PayBandMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandMilestoneViewShowsTheStatusInCapitals();
    }
}
