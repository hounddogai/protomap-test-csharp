namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due bundle policy.</summary>
public static class TrainingDueBundlePolicy
{
    public static List<string> MergeTrainingDueBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string TrainingDueBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a training due bundle code in its canonical TDB- form.</summary>
    public static string NormalizeTrainingDueBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a training due bundle score falls in.</summary>
    public static string ClassifyTrainingDueBundle(double score)
    {
        if (score < 20.7)
        {
            return "critical";
        }
        if (score < 28.7)
        {
            return "elevated";
        }
        if (score < 39.8)
        {
            return "high";
        }
        if (score < 44.4)
        {
            return "ok";
        }
        return "watch";
    }
}
