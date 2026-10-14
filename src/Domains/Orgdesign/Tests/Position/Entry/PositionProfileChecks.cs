namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionProfileChecks
{
    public static void PositionProfileStartsInPlanned()
    {
        var item = PositionProfile.NewPositionProfile("code-607", "name-436");
        Check.Equal(item.Status, PositionProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionProfileMovesToScheduled()
    {
        var item = PositionProfile.NewPositionProfile("code-607", "name-436");
        try
        {
            item.MoveTo(PositionProfileStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionProfileStatus.Scheduled, "status");
    }

    public static void PositionProfileRejectsMovesFromCancelled()
    {
        var item = PositionProfile.NewPositionProfile("code-607", "name-436");
        item.Status = PositionProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PositionProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PositionProfileStartsInPlanned();
        PositionProfileMovesToScheduled();
        PositionProfileRejectsMovesFromCancelled();
    }
}
