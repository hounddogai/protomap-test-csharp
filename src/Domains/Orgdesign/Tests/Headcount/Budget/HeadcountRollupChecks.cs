namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRollupChecks
{
    public static void HeadcountRollupStartsInIdle()
    {
        var item = HeadcountRollup.NewHeadcountRollup("code-970", "label-400");
        Check.Equal(item.Status, HeadcountRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountRollupMovesToReady()
    {
        var item = HeadcountRollup.NewHeadcountRollup("code-970", "label-400");
        try
        {
            item.MoveTo(HeadcountRollupStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountRollupStatus.Ready, "status");
    }

    public static void HeadcountRollupRejectsMovesFromStopped()
    {
        var item = HeadcountRollup.NewHeadcountRollup("code-970", "label-400");
        item.Status = HeadcountRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountRollupStartsInIdle();
        HeadcountRollupMovesToReady();
        HeadcountRollupRejectsMovesFromStopped();
    }
}
