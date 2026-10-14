namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility window policy.</summary>
public static class MobilityWindowPolicy
{
    public static double MobilityWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 80854.0, in order.</summary>
    public static int MobilityWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80854.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
