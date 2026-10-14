namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default policy cycle policy.</summary>
public static class DefaultPolicyCyclePolicy
{
    /// <summary>Puts a default policy cycle code in its canonical DPC- form.</summary>
    public static string NormalizeDefaultPolicyCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int DefaultPolicyCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPolicyCycle()
    {
        var policy = Retry.DefaultPolicy("default-policy-cycle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
