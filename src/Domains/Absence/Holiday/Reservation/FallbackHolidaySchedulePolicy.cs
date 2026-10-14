namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback holiday schedule policy.</summary>
public static class FallbackHolidaySchedulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackHolidaySchedule()
    {
        var policy = Retry.DefaultPolicy("fallback-holiday-schedule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a fallback holiday schedule code in its canonical FHS- form.</summary>
    public static string NormalizeFallbackHolidayScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FHS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FHS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 65411.0, in order.</summary>
    public static int FallbackHolidayScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65411.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
