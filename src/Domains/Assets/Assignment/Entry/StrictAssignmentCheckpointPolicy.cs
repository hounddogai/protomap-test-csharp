namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict assignment checkpoint policy.</summary>
public static class StrictAssignmentCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20456.0, in order.</summary>
    public static int StrictAssignmentCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20456.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeStrictAssignmentCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a strict assignment checkpoint code in its canonical SAC- form.</summary>
    public static string NormalizeStrictAssignmentCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
