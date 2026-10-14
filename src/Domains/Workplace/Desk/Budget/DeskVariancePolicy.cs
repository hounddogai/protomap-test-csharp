namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk variance policy.</summary>
public static class DeskVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48150.0, in order.</summary>
    public static int DeskVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48150.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DeskVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
