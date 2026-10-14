namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyRefreshSummaryChecks
{
    public static void LegacyRefreshSummaryStartsInNew()
    {
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("code-172", "label-152");
        Check.Equal(item.Status, LegacyRefreshSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRefreshSummaryMovesToDone()
    {
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("code-172", "label-152");
        try
        {
            item.MoveTo(LegacyRefreshSummaryStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRefreshSummaryStatus.Done, "status");
    }

    public static void LegacyRefreshSummaryRejectsMovesFromFailed()
    {
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("code-172", "label-152");
        item.Status = LegacyRefreshSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyRefreshSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyRefreshSummaryStartsInNew();
        LegacyRefreshSummaryMovesToDone();
        LegacyRefreshSummaryRejectsMovesFromFailed();
    }
}
