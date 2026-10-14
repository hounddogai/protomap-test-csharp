namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictHeadcountVarianceChecks
{
    public static void StrictHeadcountVarianceStartsInIdle()
    {
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("code-4", "label-193");
        Check.Equal(item.Status, StrictHeadcountVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictHeadcountVarianceMovesToReady()
    {
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("code-4", "label-193");
        try
        {
            item.MoveTo(StrictHeadcountVarianceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictHeadcountVarianceStatus.Ready, "status");
    }

    public static void StrictHeadcountVarianceRejectsMovesFromStopped()
    {
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("code-4", "label-193");
        item.Status = StrictHeadcountVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictHeadcountVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictHeadcountVarianceStartsInIdle();
        StrictHeadcountVarianceMovesToReady();
        StrictHeadcountVarianceRejectsMovesFromStopped();
    }
}
