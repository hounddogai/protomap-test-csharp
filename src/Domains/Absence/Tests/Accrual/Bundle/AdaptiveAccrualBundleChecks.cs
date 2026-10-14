namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AdaptiveAccrualBundleChecks
{
    public static void AdaptiveAccrualBundleStartsInOpen()
    {
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("code-81", "label-755");
        Check.Equal(item.Status, AdaptiveAccrualBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAccrualBundleMovesToInReview()
    {
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("code-81", "label-755");
        try
        {
            item.MoveTo(AdaptiveAccrualBundleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAccrualBundleStatus.InReview, "status");
    }

    public static void AdaptiveAccrualBundleRejectsMovesFromArchived()
    {
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("code-81", "label-755");
        item.Status = AdaptiveAccrualBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveAccrualBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveAccrualBundleStartsInOpen();
        AdaptiveAccrualBundleMovesToInReview();
        AdaptiveAccrualBundleRejectsMovesFromArchived();
    }
}
