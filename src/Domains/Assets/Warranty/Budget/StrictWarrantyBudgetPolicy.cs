namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict warranty budget policy.</summary>
public static class StrictWarrantyBudgetPolicy
{
    public static double StrictWarrantyBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict warranty budget score falls in.</summary>
    public static string ClassifyStrictWarrantyBudget(double score)
    {
        if (score < 5.5)
        {
            return "medium";
        }
        if (score < 33.1)
        {
            return "low";
        }
        if (score < 35.2)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeStrictWarrantyBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictWarrantyBudget()
    {
        var policy = Retry.DefaultPolicy("strict-warranty-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
