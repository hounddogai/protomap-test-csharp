namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position budget policy.</summary>
public static class PositionBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43809.0, in order.</summary>
    public static int PositionBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43809.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PositionBudgetSpan(int start, int end)
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
        return span / 3600;
    }

    public static string PositionBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
