namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceMilestoneChecks
{
    public static void BalanceMilestoneStartsInCreated()
    {
        var item = BalanceMilestone.NewBalanceMilestone("code-126", "label-713");
        Check.Equal(item.Status, BalanceMilestoneStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceMilestoneMovesToValidated()
    {
        var item = BalanceMilestone.NewBalanceMilestone("code-126", "label-713");
        try
        {
            item.MoveTo(BalanceMilestoneStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceMilestoneStatus.Validated, "status");
    }

    public static void BalanceMilestoneRejectsMovesFromRetired()
    {
        var item = BalanceMilestone.NewBalanceMilestone("code-126", "label-713");
        item.Status = BalanceMilestoneStatus.Retired;
        Check.Fails(() => item.MoveTo(BalanceMilestoneStatus.Created), "move");
    }

    public static void RunAll()
    {
        BalanceMilestoneStartsInCreated();
        BalanceMilestoneMovesToValidated();
        BalanceMilestoneRejectsMovesFromRetired();
    }
}
