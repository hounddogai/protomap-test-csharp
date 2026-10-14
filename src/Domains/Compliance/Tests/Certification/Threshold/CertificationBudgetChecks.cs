namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationBudgetChecks
{
    public static void CertificationBudgetStartsInDraft()
    {
        var item = CertificationBudget.NewCertificationBudget("code-837", "label-810");
        Check.Equal(item.Status, CertificationBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationBudgetMovesToPending()
    {
        var item = CertificationBudget.NewCertificationBudget("code-837", "label-810");
        try
        {
            item.MoveTo(CertificationBudgetStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationBudgetStatus.Pending, "status");
    }

    public static void CertificationBudgetRejectsMovesFromClosed()
    {
        var item = CertificationBudget.NewCertificationBudget("code-837", "label-810");
        item.Status = CertificationBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(CertificationBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CertificationBudgetStartsInDraft();
        CertificationBudgetMovesToPending();
        CertificationBudgetRejectsMovesFromClosed();
    }
}
