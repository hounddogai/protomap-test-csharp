namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class DefaultPositionThresholdChecks
{
    public static void DefaultPositionThresholdStartsInPlanned()
    {
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("code-655", "title-345");
        Check.Equal(item.Status, DefaultPositionThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPositionThresholdMovesToCancelled()
    {
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("code-655", "title-345");
        try
        {
            item.MoveTo(DefaultPositionThresholdStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPositionThresholdStatus.Cancelled, "status");
    }

    public static void DefaultPositionThresholdRejectsMovesFromCancelled()
    {
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("code-655", "title-345");
        item.Status = DefaultPositionThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultPositionThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultPositionThresholdStartsInPlanned();
        DefaultPositionThresholdMovesToCancelled();
        DefaultPositionThresholdRejectsMovesFromCancelled();
    }
}
