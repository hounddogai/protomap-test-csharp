namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment manifest policy.</summary>
public static class SentimentManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 62226.0, in order.</summary>
    public static int SentimentManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62226.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a sentiment manifest score falls in.</summary>
    public static string ClassifySentimentManifest(double score)
    {
        if (score < 37.8)
        {
            return "medium";
        }
        if (score < 42.9)
        {
            return "low";
        }
        if (score < 52.7)
        {
            return "minimal";
        }
        if (score < 81.8)
        {
            return "elevated";
        }
        return "watch";
    }

    public static double SentimentManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
