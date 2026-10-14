namespace Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Functions of offboarding schedule policy.</summary>
public static class OffboardingSchedulePolicy
{
    /// <summary>Names the band a offboarding schedule score falls in.</summary>
    public static string ClassifyOffboardingSchedule(double score)
    {
        if (score < 5.2)
        {
            return "minimal";
        }
        if (score < 25.8)
        {
            return "ok";
        }
        if (score < 30.7)
        {
            return "elevated";
        }
        if (score < 83.8)
        {
            return "watch";
        }
        return "critical";
    }

    public static int OffboardingScheduleSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 90622.0, in order.</summary>
    public static int OffboardingScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90622.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
