namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy recognition budget policy.</summary>
public static class LegacyRecognitionBudgetPolicy
{
    /// <summary>Names the band a legacy recognition budget score falls in.</summary>
    public static string ClassifyLegacyRecognitionBudget(double score)
    {
        if (score < 1.7)
        {
            return "elevated";
        }
        if (score < 32.1)
        {
            return "medium";
        }
        if (score < 56.3)
        {
            return "watch";
        }
        if (score < 85.7)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 66014.0, in order.</summary>
    public static int LegacyRecognitionBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66014.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LegacyRecognitionBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LegacyRecognitionBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
