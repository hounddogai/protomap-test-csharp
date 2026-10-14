namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineChecks
{
    public static void AdaptiveBonusPoolBaselineStartsInPlanned()
    {
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
        Check.Equal(item.Status, AdaptiveBonusPoolBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBonusPoolBaselineMovesToCancelled()
    {
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
        try
        {
            item.MoveTo(AdaptiveBonusPoolBaselineStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBonusPoolBaselineStatus.Cancelled, "status");
    }

    public static void AdaptiveBonusPoolBaselineRejectsMovesFromCancelled()
    {
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
        item.Status = AdaptiveBonusPoolBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveBonusPoolBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveBonusPoolBaselineStartsInPlanned();
        AdaptiveBonusPoolBaselineMovesToCancelled();
        AdaptiveBonusPoolBaselineRejectsMovesFromCancelled();
    }
}
