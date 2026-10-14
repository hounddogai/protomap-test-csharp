namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global payslip checkpoint policy.</summary>
public static class GlobalPayslipCheckpointPolicy
{
    /// <summary>Names the band a global payslip checkpoint score falls in.</summary>
    public static string ClassifyGlobalPayslipCheckpoint(double score)
    {
        if (score < 12.7)
        {
            return "watch";
        }
        if (score < 90.5)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 80651.0, in order.</summary>
    public static int GlobalPayslipCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80651.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalPayslipCheckpoint()
    {
        var policy = Retry.DefaultPolicy("global-payslip-checkpoint");
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
