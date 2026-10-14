namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment estimate policy.</summary>
public static class AssignmentEstimatePolicy
{
    /// <summary>Puts a assignment estimate code in its canonical AE- form.</summary>
    public static string NormalizeAssignmentEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentEstimate()
    {
        var policy = Retry.DefaultPolicy("assignment-estimate");
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

    public static int AssignmentEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
