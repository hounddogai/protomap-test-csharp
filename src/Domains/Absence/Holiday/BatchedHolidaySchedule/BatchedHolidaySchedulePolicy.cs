namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Functions of batched holiday schedule policy.</summary>
public static class BatchedHolidaySchedulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91781.0, in order.</summary>
    public static int BatchedHolidayScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91781.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BatchedHolidayScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
