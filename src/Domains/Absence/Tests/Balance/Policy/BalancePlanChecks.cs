namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BalancePlanChecks
{
    public static void BalancePlanStartsInDraft()
    {
        var item = BalancePlan.NewBalancePlan("code-967", "title-559");
        Check.Equal(item.Status, BalancePlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalancePlanMovesToPending()
    {
        var item = BalancePlan.NewBalancePlan("code-967", "title-559");
        try
        {
            item.MoveTo(BalancePlanStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalancePlanStatus.Pending, "status");
    }

    public static void BalancePlanRejectsMovesFromClosed()
    {
        var item = BalancePlan.NewBalancePlan("code-967", "title-559");
        item.Status = BalancePlanStatus.Closed;
        Check.Fails(() => item.MoveTo(BalancePlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalancePlanStartsInDraft();
        BalancePlanMovesToPending();
        BalancePlanRejectsMovesFromClosed();
    }
}
