namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work request policy.</summary>
public static class StatementOfWorkRequestPolicy
{
    /// <summary>Puts a statement of work request code in its canonical SOWR- form.</summary>
    public static string NormalizeStatementOfWorkRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkRequest()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-request");
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
