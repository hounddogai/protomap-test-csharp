namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary position profile policy.</summary>
public static class PrimaryPositionProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 46109.0, in order.</summary>
    public static int PrimaryPositionProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46109.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PrimaryPositionProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
