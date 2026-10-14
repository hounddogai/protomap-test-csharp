namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay preference policy.</summary>
public static class RetroPayPreferencePolicy
{
    public static double RetroPayPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RetroPayPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4467.0, in order.</summary>
    public static int RetroPayPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4467.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
