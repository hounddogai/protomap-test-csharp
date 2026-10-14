namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PositionThresholdChecks
{
    public static void PositionThresholdStartsInRequested()
    {
        var item = PositionThreshold.NewPositionThreshold("code-608", "label-114");
        Check.Equal(item.Status, PositionThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionThresholdMovesToExpired()
    {
        var item = PositionThreshold.NewPositionThreshold("code-608", "label-114");
        try
        {
            item.MoveTo(PositionThresholdStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionThresholdStatus.Expired, "status");
    }

    public static void PositionThresholdRejectsMovesFromExpired()
    {
        var item = PositionThreshold.NewPositionThreshold("code-608", "label-114");
        item.Status = PositionThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(PositionThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PositionThresholdStartsInRequested();
        PositionThresholdMovesToExpired();
        PositionThresholdRejectsMovesFromExpired();
    }
}
