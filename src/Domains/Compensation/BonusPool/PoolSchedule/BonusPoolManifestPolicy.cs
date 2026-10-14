namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool manifest policy.</summary>
public static class BonusPoolManifestPolicy
{
    /// <summary>Names the band a bonus pool manifest score falls in.</summary>
    public static string ClassifyBonusPoolManifest(double score)
    {
        if (score < 16.2)
        {
            return "medium";
        }
        if (score < 27.0)
        {
            return "high";
        }
        if (score < 40.4)
        {
            return "low";
        }
        return "ok";
    }

    public static List<string> MergeBonusPoolManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a bonus pool manifest code in its canonical BPM- form.</summary>
    public static string NormalizeBonusPoolManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double BonusPoolManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
