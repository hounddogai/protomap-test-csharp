namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual pay band template policy.</summary>
public static class ManualPayBandTemplatePolicy
{
    /// <summary>Names the band a manual pay band template score falls in.</summary>
    public static string ClassifyManualPayBandTemplate(double score)
    {
        if (score < 12.3)
        {
            return "elevated";
        }
        if (score < 45.6)
        {
            return "minimal";
        }
        if (score < 66.7)
        {
            return "medium";
        }
        if (score < 81.5)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeManualPayBandTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ManualPayBandTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a manual pay band template code in its canonical MPBT- form.</summary>
    public static string NormalizeManualPayBandTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MPBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MPBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
