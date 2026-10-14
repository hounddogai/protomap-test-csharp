namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy draft policy.</summary>
public static class PolicyDraftPolicy
{
    public static List<string> MergePolicyDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a policy draft code in its canonical PD- form.</summary>
    public static string NormalizePolicyDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PolicyDraftSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyDraft()
    {
        var policy = Retry.DefaultPolicy("policy-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
