namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work variance policy.</summary>
public static class StatementOfWorkVariancePolicy
{
    /// <summary>Puts a statement of work variance code in its canonical SOWV- form.</summary>
    public static string NormalizeStatementOfWorkVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StatementOfWorkVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkVariance()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-variance");
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
