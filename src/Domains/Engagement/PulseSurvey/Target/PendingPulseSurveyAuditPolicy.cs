namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending pulse survey audit policy.</summary>
public static class PendingPulseSurveyAuditPolicy
{
    /// <summary>Puts a pending pulse survey audit code in its canonical PPSA- form.</summary>
    public static string NormalizePendingPulseSurveyAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePendingPulseSurveyAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PendingPulseSurveyAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PendingPulseSurveyAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
