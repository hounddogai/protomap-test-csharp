namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient refresh request policy.</summary>
public static class LenientRefreshRequestPolicy
{
    /// <summary>Names the band a lenient refresh request score falls in.</summary>
    public static string ClassifyLenientRefreshRequest(double score)
    {
        if (score < 23.8)
        {
            return "elevated";
        }
        if (score < 74.5)
        {
            return "watch";
        }
        if (score < 97.0)
        {
            return "high";
        }
        return "critical";
    }

    public static double LenientRefreshRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 9766.0, in order.</summary>
    public static int LenientRefreshRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9766.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LenientRefreshRequestSpan(int start, int end)
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
        return span / 3600;
    }
}
