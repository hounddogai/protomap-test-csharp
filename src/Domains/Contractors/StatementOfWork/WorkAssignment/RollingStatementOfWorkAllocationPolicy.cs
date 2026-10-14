namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Functions of rolling statement of work allocation policy.</summary>
public static class RollingStatementOfWorkAllocationPolicy
{
    /// <summary>Names the band a rolling statement of work allocation score falls in.</summary>
    public static string ClassifyRollingStatementOfWorkAllocation(double score)
    {
        if (score < 9.6)
        {
            return "high";
        }
        if (score < 10.4)
        {
            return "elevated";
        }
        if (score < 52.4)
        {
            return "low";
        }
        return "critical";
    }

    public static int RollingStatementOfWorkAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
