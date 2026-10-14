namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work allocation policy.</summary>
public static class StatementOfWorkAllocationPolicy
{
    public static string StatementOfWorkAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 24652.0, in order.</summary>
    public static int StatementOfWorkAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24652.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int StatementOfWorkAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a statement of work allocation score falls in.</summary>
    public static string ClassifyStatementOfWorkAllocation(double score)
    {
        if (score < 66.5)
        {
            return "watch";
        }
        if (score < 87.5)
        {
            return "ok";
        }
        if (score < 91.5)
        {
            return "elevated";
        }
        return "minimal";
    }
}
