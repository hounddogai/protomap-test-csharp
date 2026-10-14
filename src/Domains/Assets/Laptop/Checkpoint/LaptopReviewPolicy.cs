namespace Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Functions of laptop review policy.</summary>
public static class LaptopReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20175.0, in order.</summary>
    public static int LaptopReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20175.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a laptop review score falls in.</summary>
    public static string ClassifyLaptopReview(double score)
    {
        if (score < 10.7)
        {
            return "minimal";
        }
        if (score < 17.6)
        {
            return "high";
        }
        if (score < 43.1)
        {
            return "critical";
        }
        return "medium";
    }
}
