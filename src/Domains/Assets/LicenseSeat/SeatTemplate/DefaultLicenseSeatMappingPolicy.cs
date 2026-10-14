namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Functions of default license seat mapping policy.</summary>
public static class DefaultLicenseSeatMappingPolicy
{
    public static int DefaultLicenseSeatMappingSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 64189.0, in order.</summary>
    public static int DefaultLicenseSeatMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64189.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default license seat mapping score falls in.</summary>
    public static string ClassifyDefaultLicenseSeatMapping(double score)
    {
        if (score < 41.7)
        {
            return "ok";
        }
        if (score < 45.0)
        {
            return "elevated";
        }
        if (score < 57.1)
        {
            return "high";
        }
        if (score < 73.9)
        {
            return "critical";
        }
        return "low";
    }
}
