namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Functions of adjustment proposal policy.</summary>
public static class AdjustmentProposalPolicy
{
    /// <summary>Names the band a adjustment proposal score falls in.</summary>
    public static string ClassifyAdjustmentProposal(double score)
    {
        if (score < 23.4)
        {
            return "low";
        }
        if (score < 51.5)
        {
            return "medium";
        }
        if (score < 69.3)
        {
            return "watch";
        }
        if (score < 69.4)
        {
            return "ok";
        }
        return "high";
    }

    public static int AdjustmentProposalSpan(int start, int end)
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
