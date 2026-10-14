namespace Acme.Workforce.Orgdesign.Tests.Span.RecordItem;

using Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacySpanSummaryChecks
{
    public static void LegacySpanSummaryStartsInNew()
    {
        var item = LegacySpanSummary.NewLegacySpanSummary("code-200", "name-128");
        Check.Equal(item.Status, LegacySpanSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySpanSummaryMovesToQueued()
    {
        var item = LegacySpanSummary.NewLegacySpanSummary("code-200", "name-128");
        try
        {
            item.MoveTo(LegacySpanSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySpanSummaryStatus.Queued, "status");
    }

    public static void LegacySpanSummaryRejectsMovesFromFailed()
    {
        var item = LegacySpanSummary.NewLegacySpanSummary("code-200", "name-128");
        item.Status = LegacySpanSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacySpanSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacySpanSummaryStartsInNew();
        LegacySpanSummaryMovesToQueued();
        LegacySpanSummaryRejectsMovesFromFailed();
    }
}
