namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled statement of work variance policy.</summary>
public static class ScheduledStatementOfWorkVariancePolicy
{
    /// <summary>Puts a scheduled statement of work variance code in its canonical SSOWV- form.</summary>
    public static string NormalizeScheduledStatementOfWorkVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSOWV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSOWV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledStatementOfWorkVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 12692.0, in order.</summary>
    public static int ScheduledStatementOfWorkVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12692.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledStatementOfWorkVariance()
    {
        var policy = Retry.DefaultPolicy("scheduled-statement-of-work-variance");
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
}
