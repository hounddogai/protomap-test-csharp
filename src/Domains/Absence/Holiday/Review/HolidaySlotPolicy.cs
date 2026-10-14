namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday slot policy.</summary>
public static class HolidaySlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 60736.0, in order.</summary>
    public static int HolidaySlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60736.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double HolidaySlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
