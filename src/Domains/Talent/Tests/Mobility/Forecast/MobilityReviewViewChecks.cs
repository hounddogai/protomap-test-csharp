namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewViewChecks
{
    public static void MobilityReviewViewShowsTheStatusInCapitals()
    {
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
        var view = MobilityReviewView.ToMobilityReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityReviewView.ToMobilityReviewViews(new List<MobilityReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityReviewViewShowsTheStatusInCapitals();
    }
}
