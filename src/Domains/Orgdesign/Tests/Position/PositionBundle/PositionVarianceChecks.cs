namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class PositionVarianceChecks
{
    public static void PositionVarianceStartsInPlanned()
    {
        var item = PositionVariance.NewPositionVariance("code-991", "label-899");
        Check.Equal(item.Status, PositionVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionVarianceMovesToCancelled()
    {
        var item = PositionVariance.NewPositionVariance("code-991", "label-899");
        try
        {
            item.MoveTo(PositionVarianceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionVarianceStatus.Cancelled, "status");
    }

    public static void PositionVarianceRejectsMovesFromCancelled()
    {
        var item = PositionVariance.NewPositionVariance("code-991", "label-899");
        item.Status = PositionVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PositionVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PositionVarianceStartsInPlanned();
        PositionVarianceMovesToCancelled();
        PositionVarianceRejectsMovesFromCancelled();
    }
}
