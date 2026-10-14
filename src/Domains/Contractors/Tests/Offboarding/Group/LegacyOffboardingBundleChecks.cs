namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyOffboardingBundleChecks
{
    public static void LegacyOffboardingBundleStartsInOpen()
    {
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("code-929", "label-387");
        Check.Equal(item.Status, LegacyOffboardingBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOffboardingBundleMovesToInReview()
    {
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("code-929", "label-387");
        try
        {
            item.MoveTo(LegacyOffboardingBundleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOffboardingBundleStatus.InReview, "status");
    }

    public static void LegacyOffboardingBundleRejectsMovesFromArchived()
    {
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("code-929", "label-387");
        item.Status = LegacyOffboardingBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyOffboardingBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyOffboardingBundleStartsInOpen();
        LegacyOffboardingBundleMovesToInReview();
        LegacyOffboardingBundleRejectsMovesFromArchived();
    }
}
