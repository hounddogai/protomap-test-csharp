namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Functions of fallback holiday snapshot policy.</summary>
public static class FallbackHolidaySnapshotPolicy
{
    /// <summary>Names the band a fallback holiday snapshot score falls in.</summary>
    public static string ClassifyFallbackHolidaySnapshot(double score)
    {
        if (score < 27.1)
        {
            return "elevated";
        }
        if (score < 27.2)
        {
            return "medium";
        }
        if (score < 54.6)
        {
            return "watch";
        }
        if (score < 90.1)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 43366.0, in order.</summary>
    public static int FallbackHolidaySnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43366.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
