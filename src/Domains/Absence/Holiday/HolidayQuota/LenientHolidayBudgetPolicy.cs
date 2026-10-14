namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient holiday budget policy.</summary>
public static class LenientHolidayBudgetPolicy
{
    public static string LenientHolidayBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientHolidayBudget()
    {
        var policy = Retry.DefaultPolicy("lenient-holiday-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 19012.0, in order.</summary>
    public static int LenientHolidayBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19012.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient holiday budget code in its canonical LHB- form.</summary>
    public static string NormalizeLenientHolidayBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LHB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LHB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
