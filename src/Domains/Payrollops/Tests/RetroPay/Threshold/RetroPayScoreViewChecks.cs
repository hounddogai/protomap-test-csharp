namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreViewChecks
{
    public static void RetroPayScoreViewShowsTheStatusInCapitals()
    {
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
        var view = RetroPayScoreView.ToRetroPayScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayScoreView.ToRetroPayScoreViews(new List<RetroPayScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayScoreViewShowsTheStatusInCapitals();
    }
}
