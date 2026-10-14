namespace Acme.Workforce.Engagement.Sentiment.Target;

/// <summary>Functions of sentiment target policy.</summary>
public static class SentimentTargetPolicy
{
    /// <summary>Names the band a sentiment target score falls in.</summary>
    public static string ClassifySentimentTarget(double score)
    {
        if (score < 31.3)
        {
            return "low";
        }
        if (score < 97.0)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 15788.0, in order.</summary>
    public static int SentimentTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15788.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a sentiment target code in its canonical ST- form.</summary>
    public static string NormalizeSentimentTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
