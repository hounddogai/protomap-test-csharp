namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted training due profile policy.</summary>
public static class WeightedTrainingDueProfilePolicy
{
    public static string WeightedTrainingDueProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a weighted training due profile code in its canonical WTDP- form.</summary>
    public static string NormalizeWeightedTrainingDueProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WTDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WTDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWeightedTrainingDueProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
