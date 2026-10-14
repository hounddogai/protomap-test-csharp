namespace Acme.Workforce.Orgdesign.Tests.Headcount.Rollup;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class HeadcountMetricChecks
{
    public static void HeadcountMetricStartsInRequested()
    {
        var item = HeadcountMetric.NewHeadcountMetric("code-111", "name-767");
        Check.Equal(item.Status, HeadcountMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountMetricMovesToGranted()
    {
        var item = HeadcountMetric.NewHeadcountMetric("code-111", "name-767");
        try
        {
            item.MoveTo(HeadcountMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountMetricStatus.Granted, "status");
    }

    public static void HeadcountMetricRejectsMovesFromExpired()
    {
        var item = HeadcountMetric.NewHeadcountMetric("code-111", "name-767");
        item.Status = HeadcountMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(HeadcountMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HeadcountMetricStartsInRequested();
        HeadcountMetricMovesToGranted();
        HeadcountMetricRejectsMovesFromExpired();
    }
}
