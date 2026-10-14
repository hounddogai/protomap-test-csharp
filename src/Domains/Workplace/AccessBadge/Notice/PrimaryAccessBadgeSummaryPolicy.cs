namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary access badge summary policy.</summary>
public static class PrimaryAccessBadgeSummaryPolicy
{
    public static List<string> MergePrimaryAccessBadgeSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryAccessBadgeSummary()
    {
        var policy = Retry.DefaultPolicy("primary-access-badge-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a primary access badge summary code in its canonical PABS- form.</summary>
    public static string NormalizePrimaryAccessBadgeSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PABS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PABS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
