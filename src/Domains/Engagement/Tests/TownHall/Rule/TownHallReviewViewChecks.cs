namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallReviewViewChecks
{
    public static void TownHallReviewViewShowsTheStatusInCapitals()
    {
        var item = TownHallReview.NewTownHallReview("code-757", "label-498");
        var view = TownHallReviewView.ToTownHallReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallReviewView.ToTownHallReviewViews(new List<TownHallReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallReviewViewShowsTheStatusInCapitals();
    }
}
