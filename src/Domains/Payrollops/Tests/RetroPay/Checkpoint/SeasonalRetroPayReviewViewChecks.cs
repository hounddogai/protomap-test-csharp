namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalRetroPayReviewViewChecks
{
    public static void SeasonalRetroPayReviewViewShowsTheStatusInCapitals()
    {
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("code-908", "name-603");
        var view = SeasonalRetroPayReviewView.ToSeasonalRetroPayReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalRetroPayReviewView.ToSeasonalRetroPayReviewViews(new List<SeasonalRetroPayReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalRetroPayReviewViewShowsTheStatusInCapitals();
    }
}
