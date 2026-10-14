namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary statement of work allocation policy.</summary>
public static class PrimaryStatementOfWorkAllocationPolicy
{
    public static string PrimaryStatementOfWorkAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a primary statement of work allocation score falls in.</summary>
    public static string ClassifyPrimaryStatementOfWorkAllocation(double score)
    {
        if (score < 27.3)
        {
            return "elevated";
        }
        if (score < 97.4)
        {
            return "low";
        }
        return "watch";
    }
}
