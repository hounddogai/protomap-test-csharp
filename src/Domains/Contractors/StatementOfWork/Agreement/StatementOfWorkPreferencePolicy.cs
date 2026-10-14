namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work preference policy.</summary>
public static class StatementOfWorkPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkPreference()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-preference");
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

    /// <summary>Puts a statement of work preference code in its canonical SOWP- form.</summary>
    public static string NormalizeStatementOfWorkPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StatementOfWorkPreferenceSpan(int start, int end)
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
        return span / 86400;
    }
}
