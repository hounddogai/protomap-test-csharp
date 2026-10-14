namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled retro pay score policy.</summary>
public static class ScheduledRetroPayScorePolicy
{
    /// <summary>Puts a scheduled retro pay score code in its canonical SRPS- form.</summary>
    public static string NormalizeScheduledRetroPayScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledRetroPayScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 89776.0, in order.</summary>
    public static int ScheduledRetroPayScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89776.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
