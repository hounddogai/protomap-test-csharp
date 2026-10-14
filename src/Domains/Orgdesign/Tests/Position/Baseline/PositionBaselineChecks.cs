namespace Acme.Workforce.Orgdesign.Tests.Position.Baseline;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PositionBaselineChecks
{
    public static void PositionBaselineStartsInNew()
    {
        var item = PositionBaseline.NewPositionBaseline("code-945", "label-153");
        Check.Equal(item.Status, PositionBaselineStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionBaselineMovesToRunning()
    {
        var item = PositionBaseline.NewPositionBaseline("code-945", "label-153");
        try
        {
            item.MoveTo(PositionBaselineStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionBaselineStatus.Running, "status");
    }

    public static void PositionBaselineRejectsMovesFromFailed()
    {
        var item = PositionBaseline.NewPositionBaseline("code-945", "label-153");
        item.Status = PositionBaselineStatus.Failed;
        Check.Fails(() => item.MoveTo(PositionBaselineStatus.New), "move");
    }

    public static void RunAll()
    {
        PositionBaselineStartsInNew();
        PositionBaselineMovesToRunning();
        PositionBaselineRejectsMovesFromFailed();
    }
}
