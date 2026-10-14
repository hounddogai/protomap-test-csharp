namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient assignment score policy.</summary>
public static class LenientAssignmentScorePolicy
{
    /// <summary>Names the band a lenient assignment score score falls in.</summary>
    public static string ClassifyLenientAssignmentScore(double score)
    {
        if (score < 28.0)
        {
            return "ok";
        }
        if (score < 43.9)
        {
            return "medium";
        }
        if (score < 57.6)
        {
            return "low";
        }
        if (score < 92.6)
        {
            return "critical";
        }
        return "watch";
    }

    public static int LenientAssignmentScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string LenientAssignmentScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 83377.0, in order.</summary>
    public static int LenientAssignmentScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83377.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
