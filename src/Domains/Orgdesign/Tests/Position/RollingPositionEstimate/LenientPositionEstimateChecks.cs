namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LenientPositionEstimateChecks
{
    public static void LenientPositionEstimateStartsInRequested()
    {
        var item = LenientPositionEstimate.NewLenientPositionEstimate("code-148", "name-953");
        Check.Equal(item.Status, LenientPositionEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPositionEstimateMovesToGranted()
    {
        var item = LenientPositionEstimate.NewLenientPositionEstimate("code-148", "name-953");
        try
        {
            item.MoveTo(LenientPositionEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPositionEstimateStatus.Granted, "status");
    }

    public static void LenientPositionEstimateRejectsMovesFromExpired()
    {
        var item = LenientPositionEstimate.NewLenientPositionEstimate("code-148", "name-953");
        item.Status = LenientPositionEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientPositionEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientPositionEstimateStartsInRequested();
        LenientPositionEstimateMovesToGranted();
        LenientPositionEstimateRejectsMovesFromExpired();
    }
}
