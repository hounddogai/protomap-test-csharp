namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional policy assignment policy.</summary>
public static class RegionalPolicyAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalPolicyAssignment()
    {
        var policy = Retry.DefaultPolicy("regional-policy-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a regional policy assignment code in its canonical RPA- form.</summary>
    public static string NormalizeRegionalPolicyAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
