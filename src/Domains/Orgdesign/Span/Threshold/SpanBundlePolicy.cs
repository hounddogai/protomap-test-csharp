namespace Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Functions of span bundle policy.</summary>
public static class SpanBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 60749.0, in order.</summary>
    public static int SpanBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60749.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a span bundle code in its canonical SB- form.</summary>
    public static string NormalizeSpanBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a span bundle score falls in.</summary>
    public static string ClassifySpanBundle(double score)
    {
        if (score < 45.1)
        {
            return "minimal";
        }
        if (score < 58.2)
        {
            return "watch";
        }
        return "high";
    }
}
