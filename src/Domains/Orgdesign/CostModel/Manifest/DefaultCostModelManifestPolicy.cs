namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default cost model manifest policy.</summary>
public static class DefaultCostModelManifestPolicy
{
    public static List<string> MergeDefaultCostModelManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double DefaultCostModelManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a default cost model manifest score falls in.</summary>
    public static string ClassifyDefaultCostModelManifest(double score)
    {
        if (score < 32.0)
        {
            return "high";
        }
        if (score < 33.4)
        {
            return "medium";
        }
        if (score < 86.2)
        {
            return "critical";
        }
        if (score < 93.5)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 1762.0, in order.</summary>
    public static int DefaultCostModelManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1762.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
