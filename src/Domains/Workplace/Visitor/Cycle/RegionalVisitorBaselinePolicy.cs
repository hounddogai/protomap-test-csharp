namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional visitor baseline policy.</summary>
public static class RegionalVisitorBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 61601.0, in order.</summary>
    public static int RegionalVisitorBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61601.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalVisitorBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
