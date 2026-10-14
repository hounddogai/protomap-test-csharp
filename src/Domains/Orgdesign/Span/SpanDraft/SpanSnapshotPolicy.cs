namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span snapshot policy.</summary>
public static class SpanSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35837.0, in order.</summary>
    public static int SpanSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35837.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a span snapshot score falls in.</summary>
    public static string ClassifySpanSnapshot(double score)
    {
        if (score < 37.0)
        {
            return "minimal";
        }
        if (score < 62.7)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Puts a span snapshot code in its canonical SS- form.</summary>
    public static string NormalizeSpanSnapshotCode(string code)
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

    public static List<string> MergeSpanSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
