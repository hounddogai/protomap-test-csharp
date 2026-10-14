namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop metric policy.</summary>
public static class LaptopMetricPolicy
{
    public static string LaptopMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 29692.0, in order.</summary>
    public static int LaptopMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29692.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
