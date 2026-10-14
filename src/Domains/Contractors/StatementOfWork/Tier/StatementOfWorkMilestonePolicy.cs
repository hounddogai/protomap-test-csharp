namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work milestone policy.</summary>
public static class StatementOfWorkMilestonePolicy
{
    /// <summary>Names the band a statement of work milestone score falls in.</summary>
    public static string ClassifyStatementOfWorkMilestone(double score)
    {
        if (score < 22.8)
        {
            return "minimal";
        }
        if (score < 32.9)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 51506.0, in order.</summary>
    public static int StatementOfWorkMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51506.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkMilestone()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-milestone");
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
