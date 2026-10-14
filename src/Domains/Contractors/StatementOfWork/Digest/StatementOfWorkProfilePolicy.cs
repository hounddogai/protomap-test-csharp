namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work profile policy.</summary>
public static class StatementOfWorkProfilePolicy
{
    public static double StatementOfWorkProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a statement of work profile score falls in.</summary>
    public static string ClassifyStatementOfWorkProfile(double score)
    {
        if (score < 43.0)
        {
            return "high";
        }
        if (score < 53.4)
        {
            return "watch";
        }
        if (score < 90.2)
        {
            return "ok";
        }
        return "low";
    }

    public static string StatementOfWorkProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 30696.0, in order.</summary>
    public static int StatementOfWorkProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30696.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
