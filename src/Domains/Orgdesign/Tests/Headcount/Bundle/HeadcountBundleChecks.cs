namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class HeadcountBundleChecks
{
    public static void HeadcountBundleStartsInPlanned()
    {
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
        Check.Equal(item.Status, HeadcountBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountBundleMovesToInProgress()
    {
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
        try
        {
            item.MoveTo(HeadcountBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountBundleStatus.InProgress, "status");
    }

    public static void HeadcountBundleRejectsMovesFromCancelled()
    {
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
        item.Status = HeadcountBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HeadcountBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HeadcountBundleStartsInPlanned();
        HeadcountBundleMovesToInProgress();
        HeadcountBundleRejectsMovesFromCancelled();
    }
}
