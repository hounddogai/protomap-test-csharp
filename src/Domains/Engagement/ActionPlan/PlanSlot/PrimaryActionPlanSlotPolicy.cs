namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary action plan slot policy.</summary>
public static class PrimaryActionPlanSlotPolicy
{
    /// <summary>Puts a primary action plan slot code in its canonical PAPS- form.</summary>
    public static string NormalizePrimaryActionPlanSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePrimaryActionPlanSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PrimaryActionPlanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
