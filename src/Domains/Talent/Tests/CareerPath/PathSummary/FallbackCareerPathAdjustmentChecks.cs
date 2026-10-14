namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class FallbackCareerPathAdjustmentChecks
{
    public static void FallbackCareerPathAdjustmentStartsInIdle()
    {
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
        Check.Equal(item.Status, FallbackCareerPathAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCareerPathAdjustmentMovesToDraining()
    {
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
        try
        {
            item.MoveTo(FallbackCareerPathAdjustmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCareerPathAdjustmentStatus.Draining, "status");
    }

    public static void FallbackCareerPathAdjustmentRejectsMovesFromStopped()
    {
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
        item.Status = FallbackCareerPathAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackCareerPathAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackCareerPathAdjustmentStartsInIdle();
        FallbackCareerPathAdjustmentMovesToDraining();
        FallbackCareerPathAdjustmentRejectsMovesFromStopped();
    }
}
