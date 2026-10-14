namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return rule policy.</summary>
public static class AssetReturnRulePolicy
{
    public static double AssetReturnRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a asset return rule code in its canonical ARR- form.</summary>
    public static string NormalizeAssetReturnRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAssetReturnRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 19178.0, in order.</summary>
    public static int AssetReturnRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19178.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
