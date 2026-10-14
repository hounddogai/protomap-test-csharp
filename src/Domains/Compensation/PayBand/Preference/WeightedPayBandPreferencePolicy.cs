namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted pay band preference policy.</summary>
public static class WeightedPayBandPreferencePolicy
{
    public static string WeightedPayBandPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 79618.0, in order.</summary>
    public static int WeightedPayBandPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79618.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
