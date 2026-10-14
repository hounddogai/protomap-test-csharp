namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched payslip proposal policy.</summary>
public static class BatchedPayslipProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedPayslipProposal()
    {
        var policy = Retry.DefaultPolicy("batched-payslip-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int BatchedPayslipProposalSpan(int start, int end)
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

    /// <summary>Names the band a batched payslip proposal score falls in.</summary>
    public static string ClassifyBatchedPayslipProposal(double score)
    {
        if (score < 18.2)
        {
            return "watch";
        }
        if (score < 93.9)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 59672.0, in order.</summary>
    public static int BatchedPayslipProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59672.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
