namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of action plan allocation policy.</summary>
public static class ActionPlanAllocationPolicy
{
    public static string ActionPlanAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4375.0, in order.</summary>
    public static int ActionPlanAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4375.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeActionPlanAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a action plan allocation code in its canonical APA- form.</summary>
    public static string NormalizeActionPlanAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
