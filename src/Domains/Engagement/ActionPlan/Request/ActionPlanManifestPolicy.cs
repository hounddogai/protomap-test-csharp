namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of action plan manifest policy.</summary>
public static class ActionPlanManifestPolicy
{
    /// <summary>Puts a action plan manifest code in its canonical APM- form.</summary>
    public static string NormalizeActionPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeActionPlanManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ActionPlanManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a action plan manifest score falls in.</summary>
    public static string ClassifyActionPlanManifest(double score)
    {
        if (score < 63.4)
        {
            return "low";
        }
        if (score < 79.6)
        {
            return "high";
        }
        return "watch";
    }
}
