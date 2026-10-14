namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EquityGrantReviewViewChecks
{
    public static void EquityGrantReviewViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantReview.NewEquityGrantReview("code-48", "name-281");
        var view = EquityGrantReviewView.ToEquityGrantReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantReviewView.ToEquityGrantReviewViews(new List<EquityGrantReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantReviewViewShowsTheStatusInCapitals();
    }
}
