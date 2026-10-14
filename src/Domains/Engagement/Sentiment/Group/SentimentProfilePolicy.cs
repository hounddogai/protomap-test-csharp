namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment profile policy.</summary>
public static class SentimentProfilePolicy
{
    public static string SentimentProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double SentimentProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
