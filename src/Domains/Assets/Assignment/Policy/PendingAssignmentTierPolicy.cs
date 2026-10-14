namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending assignment tier policy.</summary>
public static class PendingAssignmentTierPolicy
{
    public static double PendingAssignmentTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 28918.0, in order.</summary>
    public static int PendingAssignmentTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28918.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PendingAssignmentTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
