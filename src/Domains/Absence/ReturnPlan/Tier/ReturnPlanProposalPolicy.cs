namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan proposal policy.</summary>
public static class ReturnPlanProposalPolicy
{
    /// <summary>Puts a return plan proposal code in its canonical RPP- form.</summary>
    public static string NormalizeReturnPlanProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeReturnPlanProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ReturnPlanProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int ReturnPlanProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
