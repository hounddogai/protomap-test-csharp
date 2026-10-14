namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledRetroPayScoreViewChecks
{
    public static void ScheduledRetroPayScoreViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
        var view = ScheduledRetroPayScoreView.ToScheduledRetroPayScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRetroPayScoreView.ToScheduledRetroPayScoreViews(new List<ScheduledRetroPayScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRetroPayScoreViewShowsTheStatusInCapitals();
    }
}
