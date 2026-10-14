namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountSummaryChecks
{
    public static void HeadcountSummaryStartsInNew()
    {
        var item = HeadcountSummary.NewHeadcountSummary("code-600", "name-781");
        Check.Equal(item.Status, HeadcountSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountSummaryMovesToFailed()
    {
        var item = HeadcountSummary.NewHeadcountSummary("code-600", "name-781");
        try
        {
            item.MoveTo(HeadcountSummaryStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountSummaryStatus.Failed, "status");
    }

    public static void HeadcountSummaryRejectsMovesFromFailed()
    {
        var item = HeadcountSummary.NewHeadcountSummary("code-600", "name-781");
        item.Status = HeadcountSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(HeadcountSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        HeadcountSummaryStartsInNew();
        HeadcountSummaryMovesToFailed();
        HeadcountSummaryRejectsMovesFromFailed();
    }
}
