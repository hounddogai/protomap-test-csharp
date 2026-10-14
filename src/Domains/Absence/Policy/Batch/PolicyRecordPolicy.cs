namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy record policy.</summary>
public static class PolicyRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10241.0, in order.</summary>
    public static int PolicyRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10241.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a policy record code in its canonical PR- form.</summary>
    public static string NormalizePolicyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePolicyRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
