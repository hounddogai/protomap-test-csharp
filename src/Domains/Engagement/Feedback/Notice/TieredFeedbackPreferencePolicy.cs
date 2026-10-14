namespace Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Functions of tiered feedback preference policy.</summary>
public static class TieredFeedbackPreferencePolicy
{
    /// <summary>Names the band a tiered feedback preference score falls in.</summary>
    public static string ClassifyTieredFeedbackPreference(double score)
    {
        if (score < 4.5)
        {
            return "high";
        }
        if (score < 97.0)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Puts a tiered feedback preference code in its canonical TFP- form.</summary>
    public static string NormalizeTieredFeedbackPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredFeedbackPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 26330.0, in order.</summary>
    public static int TieredFeedbackPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26330.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
