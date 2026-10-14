namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AuditReviewChecks
{
    public static void AuditReviewStartsInPlanned()
    {
        var item = AuditReview.NewAuditReview("code-216", "title-136");
        Check.Equal(item.Status, AuditReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditReviewMovesToInProgress()
    {
        var item = AuditReview.NewAuditReview("code-216", "title-136");
        try
        {
            item.MoveTo(AuditReviewStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditReviewStatus.InProgress, "status");
    }

    public static void AuditReviewRejectsMovesFromCancelled()
    {
        var item = AuditReview.NewAuditReview("code-216", "title-136");
        item.Status = AuditReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditReviewStartsInPlanned();
        AuditReviewMovesToInProgress();
        AuditReviewRejectsMovesFromCancelled();
    }
}
