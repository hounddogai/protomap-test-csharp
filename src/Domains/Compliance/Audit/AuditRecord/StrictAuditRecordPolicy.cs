namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict audit record policy.</summary>
public static class StrictAuditRecordPolicy
{
    public static string StrictAuditRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double StrictAuditRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict audit record score falls in.</summary>
    public static string ClassifyStrictAuditRecord(double score)
    {
        if (score < 4.1)
        {
            return "minimal";
        }
        if (score < 16.0)
        {
            return "high";
        }
        if (score < 55.5)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 78934.0, in order.</summary>
    public static int StrictAuditRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78934.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
