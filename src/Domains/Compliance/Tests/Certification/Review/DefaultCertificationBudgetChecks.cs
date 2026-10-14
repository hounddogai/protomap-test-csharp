namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultCertificationBudgetChecks
{
    public static void DefaultCertificationBudgetStartsInOpen()
    {
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("code-621", "label-58");
        Check.Equal(item.Status, DefaultCertificationBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultCertificationBudgetMovesToResolved()
    {
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("code-621", "label-58");
        try
        {
            item.MoveTo(DefaultCertificationBudgetStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultCertificationBudgetStatus.Resolved, "status");
    }

    public static void DefaultCertificationBudgetRejectsMovesFromArchived()
    {
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("code-621", "label-58");
        item.Status = DefaultCertificationBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultCertificationBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultCertificationBudgetStartsInOpen();
        DefaultCertificationBudgetMovesToResolved();
        DefaultCertificationBudgetRejectsMovesFromArchived();
    }
}
