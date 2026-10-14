namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleChecks
{
    public static void PositionBundleStartsInIdle()
    {
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
        Check.Equal(item.Status, PositionBundleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionBundleMovesToDraining()
    {
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
        try
        {
            item.MoveTo(PositionBundleStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionBundleStatus.Draining, "status");
    }

    public static void PositionBundleRejectsMovesFromStopped()
    {
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
        item.Status = PositionBundleStatus.Stopped;
        Check.Fails(() => item.MoveTo(PositionBundleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PositionBundleStartsInIdle();
        PositionBundleMovesToDraining();
        PositionBundleRejectsMovesFromStopped();
    }
}
