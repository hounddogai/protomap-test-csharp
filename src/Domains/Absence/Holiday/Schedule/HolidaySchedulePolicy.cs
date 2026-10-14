namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday schedule policy.</summary>
public static class HolidaySchedulePolicy
{
    /// <summary>Puts a holiday schedule code in its canonical HS- form.</summary>
    public static string NormalizeHolidayScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 76214.0, in order.</summary>
    public static int HolidayScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76214.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double HolidayScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int HolidayScheduleSpan(int start, int end)
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
        return span / 86400;
    }
}
