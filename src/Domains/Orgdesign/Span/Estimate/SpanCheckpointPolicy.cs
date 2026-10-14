namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span checkpoint policy.</summary>
public static class SpanCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20579.0, in order.</summary>
    public static int SpanCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20579.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a span checkpoint code in its canonical SC- form.</summary>
    public static string NormalizeSpanCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SpanCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a span checkpoint score falls in.</summary>
    public static string ClassifySpanCheckpoint(double score)
    {
        if (score < 11.7)
        {
            return "watch";
        }
        if (score < 19.0)
        {
            return "minimal";
        }
        if (score < 43.6)
        {
            return "elevated";
        }
        return "medium";
    }
}
