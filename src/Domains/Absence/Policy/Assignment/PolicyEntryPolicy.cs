namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy entry policy.</summary>
public static class PolicyEntryPolicy
{
    public static string PolicyEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 78697.0, in order.</summary>
    public static int PolicyEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78697.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
