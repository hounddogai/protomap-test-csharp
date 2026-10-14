namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantAgreementChecks
{
    public static void EquityGrantAgreementStartsInOpen()
    {
        var item = EquityGrantAgreement.NewEquityGrantAgreement("code-471", "name-836");
        Check.Equal(item.Status, EquityGrantAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantAgreementMovesToResolved()
    {
        var item = EquityGrantAgreement.NewEquityGrantAgreement("code-471", "name-836");
        try
        {
            item.MoveTo(EquityGrantAgreementStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantAgreementStatus.Resolved, "status");
    }

    public static void EquityGrantAgreementRejectsMovesFromArchived()
    {
        var item = EquityGrantAgreement.NewEquityGrantAgreement("code-471", "name-836");
        item.Status = EquityGrantAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantAgreementStartsInOpen();
        EquityGrantAgreementMovesToResolved();
        EquityGrantAgreementRejectsMovesFromArchived();
    }
}
