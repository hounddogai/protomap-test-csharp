namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy town hall schedule policy.</summary>
public static class LegacyTownHallSchedulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 11127.0, in order.</summary>
    public static int LegacyTownHallScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 11127.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy town hall schedule score falls in.</summary>
    public static string ClassifyLegacyTownHallSchedule(double score)
    {
        if (score < 15.6)
        {
            return "high";
        }
        if (score < 69.7)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Puts a legacy town hall schedule code in its canonical LTHS- form.</summary>
    public static string NormalizeLegacyTownHallScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTHS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTHS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyTownHallScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
