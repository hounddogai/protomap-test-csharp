namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall score policy.</summary>
public static class TownHallScorePolicy
{
    public static string TownHallScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double TownHallScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a town hall score score falls in.</summary>
    public static string ClassifyTownHallScore(double score)
    {
        if (score < 19.2)
        {
            return "minimal";
        }
        if (score < 28.9)
        {
            return "low";
        }
        if (score < 86.5)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 22903.0, in order.</summary>
    public static int TownHallScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22903.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
