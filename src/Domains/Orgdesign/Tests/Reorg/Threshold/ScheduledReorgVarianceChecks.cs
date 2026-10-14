namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledReorgVarianceChecks
{
    public static void ScheduledReorgVarianceStartsInRequested()
    {
        var item = ScheduledReorgVariance.NewScheduledReorgVariance("code-363", "label-857");
        Check.Equal(item.Status, ScheduledReorgVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledReorgVarianceMovesToExpired()
    {
        var item = ScheduledReorgVariance.NewScheduledReorgVariance("code-363", "label-857");
        try
        {
            item.MoveTo(ScheduledReorgVarianceStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledReorgVarianceStatus.Expired, "status");
    }

    public static void ScheduledReorgVarianceRejectsMovesFromExpired()
    {
        var item = ScheduledReorgVariance.NewScheduledReorgVariance("code-363", "label-857");
        item.Status = ScheduledReorgVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledReorgVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledReorgVarianceStartsInRequested();
        ScheduledReorgVarianceMovesToExpired();
        ScheduledReorgVarianceRejectsMovesFromExpired();
    }
}
