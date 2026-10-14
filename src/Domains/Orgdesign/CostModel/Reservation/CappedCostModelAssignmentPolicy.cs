namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped cost model assignment policy.</summary>
public static class CappedCostModelAssignmentPolicy
{
    /// <summary>Names the band a capped cost model assignment score falls in.</summary>
    public static string ClassifyCappedCostModelAssignment(double score)
    {
        if (score < 38.5)
        {
            return "watch";
        }
        if (score < 40.1)
        {
            return "low";
        }
        if (score < 80.8)
        {
            return "critical";
        }
        return "elevated";
    }

    public static string CappedCostModelAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a capped cost model assignment code in its canonical CCMA- form.</summary>
    public static string NormalizeCappedCostModelAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CCMA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CCMA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 32904.0, in order.</summary>
    public static int CappedCostModelAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32904.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
