namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool tier policy.</summary>
public static class BonusPoolTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 89400.0, in order.</summary>
    public static int BonusPoolTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89400.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a bonus pool tier score falls in.</summary>
    public static string ClassifyBonusPoolTier(double score)
    {
        if (score < 35.4)
        {
            return "elevated";
        }
        if (score < 65.9)
        {
            return "medium";
        }
        if (score < 77.5)
        {
            return "ok";
        }
        if (score < 94.6)
        {
            return "critical";
        }
        return "high";
    }

    public static List<string> MergeBonusPoolTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a bonus pool tier code in its canonical BPT- form.</summary>
    public static string NormalizeBonusPoolTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
