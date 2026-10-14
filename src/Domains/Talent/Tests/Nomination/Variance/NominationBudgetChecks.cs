namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class NominationBudgetChecks
{
    public static void NominationBudgetStartsInDraft()
    {
        var item = NominationBudget.NewNominationBudget("code-557", "title-93");
        Check.Equal(item.Status, NominationBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationBudgetMovesToPending()
    {
        var item = NominationBudget.NewNominationBudget("code-557", "title-93");
        try
        {
            item.MoveTo(NominationBudgetStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationBudgetStatus.Pending, "status");
    }

    public static void NominationBudgetRejectsMovesFromClosed()
    {
        var item = NominationBudget.NewNominationBudget("code-557", "title-93");
        item.Status = NominationBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationBudgetStartsInDraft();
        NominationBudgetMovesToPending();
        NominationBudgetRejectsMovesFromClosed();
    }
}
