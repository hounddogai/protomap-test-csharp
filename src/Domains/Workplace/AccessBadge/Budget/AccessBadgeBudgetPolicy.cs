namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge budget policy.</summary>
public static class AccessBadgeBudgetPolicy
{
    /// <summary>Puts a access badge budget code in its canonical ABB- form.</summary>
    public static string NormalizeAccessBadgeBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAccessBadgeBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeBudget()
    {
        var policy = Retry.DefaultPolicy("access-badge-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
