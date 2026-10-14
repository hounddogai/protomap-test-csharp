namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position score policy.</summary>
public static class PositionScorePolicy
{
    public static string PositionScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 17244.0, in order.</summary>
    public static int PositionScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17244.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
