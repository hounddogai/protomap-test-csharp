namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileChecks
{
    public static void PrimaryPositionProfileStartsInIdle()
    {
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
        Check.Equal(item.Status, PrimaryPositionProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPositionProfileMovesToWarming()
    {
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
        try
        {
            item.MoveTo(PrimaryPositionProfileStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPositionProfileStatus.Warming, "status");
    }

    public static void PrimaryPositionProfileRejectsMovesFromStopped()
    {
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
        item.Status = PrimaryPositionProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryPositionProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryPositionProfileStartsInIdle();
        PrimaryPositionProfileMovesToWarming();
        PrimaryPositionProfileRejectsMovesFromStopped();
    }
}
