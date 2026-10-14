namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment schedule policy.</summary>
public static class SentimentSchedulePolicy
{
    public static string SentimentScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a sentiment schedule code in its canonical SS- form.</summary>
    public static string NormalizeSentimentScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SentimentScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 9094.0, in order.</summary>
    public static int SentimentScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9094.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
