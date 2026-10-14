namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimarySentimentRollupChecks
{
    public static void PrimarySentimentRollupStartsInCreated()
    {
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("code-465", "label-528");
        Check.Equal(item.Status, PrimarySentimentRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySentimentRollupMovesToValidated()
    {
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("code-465", "label-528");
        try
        {
            item.MoveTo(PrimarySentimentRollupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySentimentRollupStatus.Validated, "status");
    }

    public static void PrimarySentimentRollupRejectsMovesFromRetired()
    {
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("code-465", "label-528");
        item.Status = PrimarySentimentRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimarySentimentRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimarySentimentRollupStartsInCreated();
        PrimarySentimentRollupMovesToValidated();
        PrimarySentimentRollupRejectsMovesFromRetired();
    }
}
