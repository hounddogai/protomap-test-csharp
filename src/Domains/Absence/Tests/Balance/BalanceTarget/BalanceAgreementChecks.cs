namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceAgreementChecks
{
    public static void BalanceAgreementStartsInDraft()
    {
        var item = BalanceAgreement.NewBalanceAgreement("code-766", "label-479");
        Check.Equal(item.Status, BalanceAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceAgreementMovesToPending()
    {
        var item = BalanceAgreement.NewBalanceAgreement("code-766", "label-479");
        try
        {
            item.MoveTo(BalanceAgreementStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceAgreementStatus.Pending, "status");
    }

    public static void BalanceAgreementRejectsMovesFromClosed()
    {
        var item = BalanceAgreement.NewBalanceAgreement("code-766", "label-479");
        item.Status = BalanceAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(BalanceAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalanceAgreementStartsInDraft();
        BalanceAgreementMovesToPending();
        BalanceAgreementRejectsMovesFromClosed();
    }
}
