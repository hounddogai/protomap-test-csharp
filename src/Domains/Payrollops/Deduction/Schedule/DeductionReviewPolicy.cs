namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction review policy.</summary>
public static class DeductionReviewPolicy
{
    /// <summary>Puts a deduction review code in its canonical DR- form.</summary>
    public static string NormalizeDeductionReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDeductionReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionReview()
    {
        var policy = Retry.DefaultPolicy("deduction-review");
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
