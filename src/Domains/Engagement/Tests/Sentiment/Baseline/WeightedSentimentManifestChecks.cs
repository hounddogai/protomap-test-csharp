namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedSentimentManifestChecks
{
    public static void WeightedSentimentManifestStartsInCreated()
    {
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
        Check.Equal(item.Status, WeightedSentimentManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedSentimentManifestMovesToValidated()
    {
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
        try
        {
            item.MoveTo(WeightedSentimentManifestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedSentimentManifestStatus.Validated, "status");
    }

    public static void WeightedSentimentManifestRejectsMovesFromRetired()
    {
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
        item.Status = WeightedSentimentManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedSentimentManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedSentimentManifestStartsInCreated();
        WeightedSentimentManifestMovesToValidated();
        WeightedSentimentManifestRejectsMovesFromRetired();
    }
}
