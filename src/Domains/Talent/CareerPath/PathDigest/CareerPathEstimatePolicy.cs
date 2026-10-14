namespace Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Functions of career path estimate policy.</summary>
public static class CareerPathEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 69126.0, in order.</summary>
    public static int CareerPathEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69126.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CareerPathEstimateSpan(int start, int end)
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
