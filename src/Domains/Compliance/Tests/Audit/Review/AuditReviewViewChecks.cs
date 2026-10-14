namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AuditReviewViewChecks
{
    public static void AuditReviewViewShowsTheStatusInCapitals()
    {
        var item = AuditReview.NewAuditReview("code-216", "title-136");
        var view = AuditReviewView.ToAuditReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditReviewView.ToAuditReviewViews(new List<AuditReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditReviewViewShowsTheStatusInCapitals();
    }
}
