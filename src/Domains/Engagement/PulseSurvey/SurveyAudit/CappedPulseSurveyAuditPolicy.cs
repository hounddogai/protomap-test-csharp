namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pulse survey audit policy.</summary>
public static class CappedPulseSurveyAuditPolicy
{
    /// <summary>Puts a capped pulse survey audit code in its canonical CPSA- form.</summary>
    public static string NormalizeCappedPulseSurveyAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped pulse survey audit score falls in.</summary>
    public static string ClassifyCappedPulseSurveyAudit(double score)
    {
        if (score < 12.3)
        {
            return "high";
        }
        if (score < 25.1)
        {
            return "watch";
        }
        return "ok";
    }

    public static List<string> MergeCappedPulseSurveyAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double CappedPulseSurveyAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
