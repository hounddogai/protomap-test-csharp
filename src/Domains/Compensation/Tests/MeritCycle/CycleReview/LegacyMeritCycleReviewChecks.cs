namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewChecks
{
    public static void LegacyMeritCycleReviewStartsInPlanned()
    {
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
        Check.Equal(item.Status, LegacyMeritCycleReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyMeritCycleReviewMovesToScheduled()
    {
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
        try
        {
            item.MoveTo(LegacyMeritCycleReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyMeritCycleReviewStatus.Scheduled, "status");
    }

    public static void LegacyMeritCycleReviewRejectsMovesFromCancelled()
    {
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
        item.Status = LegacyMeritCycleReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyMeritCycleReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyMeritCycleReviewStartsInPlanned();
        LegacyMeritCycleReviewMovesToScheduled();
        LegacyMeritCycleReviewRejectsMovesFromCancelled();
    }
}
