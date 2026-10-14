namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient retro pay preference policy.</summary>
public static class LenientRetroPayPreferencePolicy
{
    /// <summary>Names the band a lenient retro pay preference score falls in.</summary>
    public static string ClassifyLenientRetroPayPreference(double score)
    {
        if (score < 68.9)
        {
            return "minimal";
        }
        if (score < 72.4)
        {
            return "watch";
        }
        if (score < 79.5)
        {
            return "ok";
        }
        return "high";
    }

    public static int LenientRetroPayPreferenceSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 74745.0, in order.</summary>
    public static int LenientRetroPayPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74745.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LenientRetroPayPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
