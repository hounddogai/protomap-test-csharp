namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model score policy.</summary>
public static class CostModelScorePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 64090.0, in order.</summary>
    public static int CostModelScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64090.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CostModelScoreSpan(int start, int end)
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
        return span / 60;
    }

    public static double CostModelScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
