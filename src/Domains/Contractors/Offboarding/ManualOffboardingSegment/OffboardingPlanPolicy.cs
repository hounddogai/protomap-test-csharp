namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of offboarding plan policy.</summary>
public static class OffboardingPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45980.0, in order.</summary>
    public static int OffboardingPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45980.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int OffboardingPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a offboarding plan code in its canonical OP- form.</summary>
    public static string NormalizeOffboardingPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeOffboardingPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
