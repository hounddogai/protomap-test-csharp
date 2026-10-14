namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary laptop checkpoint policy.</summary>
public static class PrimaryLaptopCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 18330.0, in order.</summary>
    public static int PrimaryLaptopCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18330.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PrimaryLaptopCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a primary laptop checkpoint score falls in.</summary>
    public static string ClassifyPrimaryLaptopCheckpoint(double score)
    {
        if (score < 6.8)
        {
            return "watch";
        }
        if (score < 84.2)
        {
            return "critical";
        }
        if (score < 96.3)
        {
            return "elevated";
        }
        return "low";
    }
}
