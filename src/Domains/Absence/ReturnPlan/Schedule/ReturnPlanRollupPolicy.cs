namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan rollup policy.</summary>
public static class ReturnPlanRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70905.0, in order.</summary>
    public static int ReturnPlanRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70905.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a return plan rollup code in its canonical RPR- form.</summary>
    public static string NormalizeReturnPlanRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeReturnPlanRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ReturnPlanRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
