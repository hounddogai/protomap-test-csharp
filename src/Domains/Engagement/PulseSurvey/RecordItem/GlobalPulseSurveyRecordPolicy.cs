namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global pulse survey record policy.</summary>
public static class GlobalPulseSurveyRecordPolicy
{
    /// <summary>Puts a global pulse survey record code in its canonical GPSR- form.</summary>
    public static string NormalizeGlobalPulseSurveyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalPulseSurveyRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string GlobalPulseSurveyRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeGlobalPulseSurveyRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
