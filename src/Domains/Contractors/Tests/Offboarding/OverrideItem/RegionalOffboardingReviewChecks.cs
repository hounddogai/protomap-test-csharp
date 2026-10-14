namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalOffboardingReviewChecks
{
    public static void RegionalOffboardingReviewStartsInProposed()
    {
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("code-997", "title-440");
        Check.Equal(item.Status, RegionalOffboardingReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalOffboardingReviewMovesToApproved()
    {
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("code-997", "title-440");
        try
        {
            item.MoveTo(RegionalOffboardingReviewStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalOffboardingReviewStatus.Approved, "status");
    }

    public static void RegionalOffboardingReviewRejectsMovesFromWithdrawn()
    {
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("code-997", "title-440");
        item.Status = RegionalOffboardingReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalOffboardingReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalOffboardingReviewStartsInProposed();
        RegionalOffboardingReviewMovesToApproved();
        RegionalOffboardingReviewRejectsMovesFromWithdrawn();
    }
}
