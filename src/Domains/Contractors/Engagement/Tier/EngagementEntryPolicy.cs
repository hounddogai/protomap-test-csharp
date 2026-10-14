namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement entry policy.</summary>
public static class EngagementEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementEntry()
    {
        var policy = Retry.DefaultPolicy("engagement-entry");
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

    /// <summary>Puts a engagement entry code in its canonical EE- form.</summary>
    public static string NormalizeEngagementEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
