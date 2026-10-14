namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalMobilityReviewViewChecks
{
    public static void RegionalMobilityReviewViewShowsTheStatusInCapitals()
    {
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
        var view = RegionalMobilityReviewView.ToRegionalMobilityReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalMobilityReviewView.ToRegionalMobilityReviewViews(new List<RegionalMobilityReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalMobilityReviewViewShowsTheStatusInCapitals();
    }
}
