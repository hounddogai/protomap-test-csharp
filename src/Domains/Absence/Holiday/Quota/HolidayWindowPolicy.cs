namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday window policy.</summary>
public static class HolidayWindowPolicy
{
    /// <summary>Names the band a holiday window score falls in.</summary>
    public static string ClassifyHolidayWindow(double score)
    {
        if (score < 20.2)
        {
            return "low";
        }
        if (score < 35.9)
        {
            return "minimal";
        }
        if (score < 52.3)
        {
            return "high";
        }
        if (score < 71.2)
        {
            return "watch";
        }
        return "ok";
    }

    public static int HolidayWindowSpan(int start, int end)
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
        return span / 60;
    }

    public static double HolidayWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 23760.0, in order.</summary>
    public static int HolidayWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23760.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
