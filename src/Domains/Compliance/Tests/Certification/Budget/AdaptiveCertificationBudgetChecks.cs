namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveCertificationBudgetChecks
{
    public static void AdaptiveCertificationBudgetStartsInDraft()
    {
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("code-954", "name-148");
        Check.Equal(item.Status, AdaptiveCertificationBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCertificationBudgetMovesToActive()
    {
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("code-954", "name-148");
        try
        {
            item.MoveTo(AdaptiveCertificationBudgetStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCertificationBudgetStatus.Active, "status");
    }

    public static void AdaptiveCertificationBudgetRejectsMovesFromClosed()
    {
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("code-954", "name-148");
        item.Status = AdaptiveCertificationBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveCertificationBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveCertificationBudgetStartsInDraft();
        AdaptiveCertificationBudgetMovesToActive();
        AdaptiveCertificationBudgetRejectsMovesFromClosed();
    }
}
