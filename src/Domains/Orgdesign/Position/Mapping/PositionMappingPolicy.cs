namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position mapping policy.</summary>
public static class PositionMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 13488.0, in order.</summary>
    public static int PositionMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13488.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PositionMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PositionMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
