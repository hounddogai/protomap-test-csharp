namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path audit policy.</summary>
public static class CareerPathAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathAudit()
    {
        var policy = Retry.DefaultPolicy("career-path-audit");
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

    public static int CareerPathAuditSpan(int start, int end)
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

    /// <summary>Puts a career path audit code in its canonical CPA- form.</summary>
    public static string NormalizeCareerPathAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
