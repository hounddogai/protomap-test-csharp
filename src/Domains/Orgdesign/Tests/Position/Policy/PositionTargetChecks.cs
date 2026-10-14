namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionTargetChecks
{
    public static void PositionTargetStartsInCreated()
    {
        var item = PositionTarget.NewPositionTarget("code-868", "name-253");
        Check.Equal(item.Status, PositionTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionTargetMovesToPublished()
    {
        var item = PositionTarget.NewPositionTarget("code-868", "name-253");
        try
        {
            item.MoveTo(PositionTargetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionTargetStatus.Published, "status");
    }

    public static void PositionTargetRejectsMovesFromRetired()
    {
        var item = PositionTarget.NewPositionTarget("code-868", "name-253");
        item.Status = PositionTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(PositionTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        PositionTargetStartsInCreated();
        PositionTargetMovesToPublished();
        PositionTargetRejectsMovesFromRetired();
    }
}
