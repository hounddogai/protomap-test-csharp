namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback laptop notice policy.</summary>
public static class FallbackLaptopNoticePolicy
{
    public static double FallbackLaptopNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 1885.0, in order.</summary>
    public static int FallbackLaptopNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1885.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
