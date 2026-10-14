namespace Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Functions of lenient visitor checkpoint policy.</summary>
public static class LenientVisitorCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 6724.0, in order.</summary>
    public static int LenientVisitorCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6724.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient visitor checkpoint score falls in.</summary>
    public static string ClassifyLenientVisitorCheckpoint(double score)
    {
        if (score < 5.7)
        {
            return "elevated";
        }
        if (score < 13.5)
        {
            return "low";
        }
        if (score < 37.3)
        {
            return "ok";
        }
        if (score < 50.0)
        {
            return "medium";
        }
        return "minimal";
    }

    public static int LenientVisitorCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
