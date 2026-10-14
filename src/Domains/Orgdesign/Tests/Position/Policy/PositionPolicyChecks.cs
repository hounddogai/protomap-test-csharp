namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionPolicyChecks
{
    public static void PositionPolicyStartsInIdle()
    {
        var item = PositionPolicy.NewPositionPolicy("code-840", "label-164");
        Check.Equal(item.Status, PositionPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionPolicyMovesToReady()
    {
        var item = PositionPolicy.NewPositionPolicy("code-840", "label-164");
        try
        {
            item.MoveTo(PositionPolicyStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionPolicyStatus.Ready, "status");
    }

    public static void PositionPolicyRejectsMovesFromStopped()
    {
        var item = PositionPolicy.NewPositionPolicy("code-840", "label-164");
        item.Status = PositionPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(PositionPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PositionPolicyStartsInIdle();
        PositionPolicyMovesToReady();
        PositionPolicyRejectsMovesFromStopped();
    }
}
