namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyAdjustmentChecks
{
    public static void PolicyAdjustmentStartsInOpen()
    {
        var item = PolicyAdjustment.NewPolicyAdjustment("code-346", "title-519");
        Check.Equal(item.Status, PolicyAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAdjustmentMovesToResolved()
    {
        var item = PolicyAdjustment.NewPolicyAdjustment("code-346", "title-519");
        try
        {
            item.MoveTo(PolicyAdjustmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAdjustmentStatus.Resolved, "status");
    }

    public static void PolicyAdjustmentRejectsMovesFromArchived()
    {
        var item = PolicyAdjustment.NewPolicyAdjustment("code-346", "title-519");
        item.Status = PolicyAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyAdjustmentStartsInOpen();
        PolicyAdjustmentMovesToResolved();
        PolicyAdjustmentRejectsMovesFromArchived();
    }
}
