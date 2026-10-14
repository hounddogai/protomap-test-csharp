namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient garnishment slot policy.</summary>
public static class LenientGarnishmentSlotPolicy
{
    /// <summary>Names the band a lenient garnishment slot score falls in.</summary>
    public static string ClassifyLenientGarnishmentSlot(double score)
    {
        if (score < 79.8)
        {
            return "ok";
        }
        if (score < 87.1)
        {
            return "critical";
        }
        if (score < 99.2)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 92810.0, in order.</summary>
    public static int LenientGarnishmentSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92810.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientGarnishmentSlot()
    {
        var policy = Retry.DefaultPolicy("lenient-garnishment-slot");
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
