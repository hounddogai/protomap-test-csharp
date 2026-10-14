namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanChecks
{
    public static void LenientBalancePlanStartsInPlanned()
    {
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
        Check.Equal(item.Status, LenientBalancePlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBalancePlanMovesToScheduled()
    {
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
        try
        {
            item.MoveTo(LenientBalancePlanStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBalancePlanStatus.Scheduled, "status");
    }

    public static void LenientBalancePlanRejectsMovesFromCancelled()
    {
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
        item.Status = LenientBalancePlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientBalancePlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientBalancePlanStartsInPlanned();
        LenientBalancePlanMovesToScheduled();
        LenientBalancePlanRejectsMovesFromCancelled();
    }
}
