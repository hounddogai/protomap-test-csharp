namespace Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Functions of batched laptop request policy.</summary>
public static class BatchedLaptopRequestPolicy
{
    public static int BatchedLaptopRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 84417.0, in order.</summary>
    public static int BatchedLaptopRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84417.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
