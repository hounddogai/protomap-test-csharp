namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PositionCycleChecks
{
    public static void PositionCycleStartsInProposed()
    {
        var item = PositionCycle.NewPositionCycle("code-860", "title-551");
        Check.Equal(item.Status, PositionCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionCycleMovesToApproved()
    {
        var item = PositionCycle.NewPositionCycle("code-860", "title-551");
        try
        {
            item.MoveTo(PositionCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionCycleStatus.Approved, "status");
    }

    public static void PositionCycleRejectsMovesFromWithdrawn()
    {
        var item = PositionCycle.NewPositionCycle("code-860", "title-551");
        item.Status = PositionCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PositionCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PositionCycleStartsInProposed();
        PositionCycleMovesToApproved();
        PositionCycleRejectsMovesFromWithdrawn();
    }
}
