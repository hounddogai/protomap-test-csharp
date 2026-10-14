namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class WeightedRetroPayNoticeViewChecks
{
    public static void WeightedRetroPayNoticeViewShowsTheStatusInCapitals()
    {
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("code-201", "label-723");
        var view = WeightedRetroPayNoticeView.ToWeightedRetroPayNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedRetroPayNoticeView.ToWeightedRetroPayNoticeViews(new List<WeightedRetroPayNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedRetroPayNoticeViewShowsTheStatusInCapitals();
    }
}
