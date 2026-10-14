namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due policy policy.</summary>
public static class TrainingDuePolicyPolicy
{
    /// <summary>Names the band a training due policy score falls in.</summary>
    public static string ClassifyTrainingDuePolicy(double score)
    {
        if (score < 47.0)
        {
            return "watch";
        }
        if (score < 47.0)
        {
            return "ok";
        }
        if (score < 49.8)
        {
            return "high";
        }
        if (score < 63.8)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static double TrainingDuePolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTrainingDuePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a training due policy code in its canonical TDP- form.</summary>
    public static string NormalizeTrainingDuePolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
