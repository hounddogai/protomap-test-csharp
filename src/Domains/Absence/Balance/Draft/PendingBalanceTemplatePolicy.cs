namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending balance template policy.</summary>
public static class PendingBalanceTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingBalanceTemplate()
    {
        var policy = Retry.DefaultPolicy("pending-balance-template");
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

    /// <summary>Names the band a pending balance template score falls in.</summary>
    public static string ClassifyPendingBalanceTemplate(double score)
    {
        if (score < 4.2)
        {
            return "elevated";
        }
        if (score < 10.5)
        {
            return "minimal";
        }
        if (score < 91.7)
        {
            return "critical";
        }
        return "ok";
    }
}
