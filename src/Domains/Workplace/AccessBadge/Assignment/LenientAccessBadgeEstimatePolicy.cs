namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient access badge estimate policy.</summary>
public static class LenientAccessBadgeEstimatePolicy
{
    public static List<string> MergeLenientAccessBadgeEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient access badge estimate code in its canonical LABE- form.</summary>
    public static string NormalizeLenientAccessBadgeEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LABE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LABE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 38487.0, in order.</summary>
    public static int LenientAccessBadgeEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38487.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientAccessBadgeEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
