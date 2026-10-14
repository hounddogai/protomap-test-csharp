namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position batch policy.</summary>
public static class PositionBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35817.0, in order.</summary>
    public static int PositionBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35817.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PositionBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
