namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient statement of work bundle policy.</summary>
public static class LenientStatementOfWorkBundlePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientStatementOfWorkBundle()
    {
        var policy = Retry.DefaultPolicy("lenient-statement-of-work-bundle");
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

    /// <summary>Puts a lenient statement of work bundle code in its canonical LSOWB- form.</summary>
    public static string NormalizeLenientStatementOfWorkBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSOWB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSOWB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
