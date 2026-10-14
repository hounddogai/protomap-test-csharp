namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band preference policy.</summary>
public static class PayBandPreferencePolicy
{
    public static string PayBandPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 49227.0, in order.</summary>
    public static int PayBandPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49227.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pay band preference score falls in.</summary>
    public static string ClassifyPayBandPreference(double score)
    {
        if (score < 47.8)
        {
            return "ok";
        }
        if (score < 60.8)
        {
            return "high";
        }
        if (score < 67.4)
        {
            return "minimal";
        }
        if (score < 82.2)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandPreference()
    {
        var policy = Retry.DefaultPolicy("pay-band-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
