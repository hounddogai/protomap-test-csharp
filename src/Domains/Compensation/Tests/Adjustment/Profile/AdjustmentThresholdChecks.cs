namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentThresholdChecks
{
    public static void AdjustmentThresholdStartsInRequested()
    {
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
        Check.Equal(item.Status, AdjustmentThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentThresholdMovesToGranted()
    {
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
        try
        {
            item.MoveTo(AdjustmentThresholdStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentThresholdStatus.Granted, "status");
    }

    public static void AdjustmentThresholdRejectsMovesFromExpired()
    {
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
        item.Status = AdjustmentThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(AdjustmentThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdjustmentThresholdStartsInRequested();
        AdjustmentThresholdMovesToGranted();
        AdjustmentThresholdRejectsMovesFromExpired();
    }
}
