namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk ledger policy.</summary>
public static class DeskLedgerPolicy
{
    /// <summary>Puts a desk ledger code in its canonical DL- form.</summary>
    public static string NormalizeDeskLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskLedger()
    {
        var policy = Retry.DefaultPolicy("desk-ledger");
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
}
