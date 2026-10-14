namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default policy assignment policy.</summary>
public static class DefaultPolicyAssignmentPolicy
{
    /// <summary>Puts a default policy assignment code in its canonical DPA- form.</summary>
    public static string NormalizeDefaultPolicyAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int DefaultPolicyAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeDefaultPolicyAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double DefaultPolicyAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
