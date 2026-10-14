namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledBalanceAgreementChecks
{
    public static void ScheduledBalanceAgreementStartsInDraft()
    {
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("code-503", "title-690");
        Check.Equal(item.Status, ScheduledBalanceAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBalanceAgreementMovesToPending()
    {
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("code-503", "title-690");
        try
        {
            item.MoveTo(ScheduledBalanceAgreementStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBalanceAgreementStatus.Pending, "status");
    }

    public static void ScheduledBalanceAgreementRejectsMovesFromClosed()
    {
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("code-503", "title-690");
        item.Status = ScheduledBalanceAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledBalanceAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledBalanceAgreementStartsInDraft();
        ScheduledBalanceAgreementMovesToPending();
        ScheduledBalanceAgreementRejectsMovesFromClosed();
    }
}
