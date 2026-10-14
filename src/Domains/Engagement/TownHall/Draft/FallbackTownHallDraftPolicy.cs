namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback town hall draft policy.</summary>
public static class FallbackTownHallDraftPolicy
{
    public static string FallbackTownHallDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 42261.0, in order.</summary>
    public static int FallbackTownHallDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42261.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a fallback town hall draft score falls in.</summary>
    public static string ClassifyFallbackTownHallDraft(double score)
    {
        if (score < 20.1)
        {
            return "critical";
        }
        if (score < 59.7)
        {
            return "low";
        }
        if (score < 72.5)
        {
            return "high";
        }
        if (score < 87.7)
        {
            return "minimal";
        }
        return "ok";
    }

    public static double FallbackTownHallDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
