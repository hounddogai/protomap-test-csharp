namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment reservation policy.</summary>
public static class AdjustmentReservationPolicy
{
    /// <summary>Names the band a adjustment reservation score falls in.</summary>
    public static string ClassifyAdjustmentReservation(double score)
    {
        if (score < 34.5)
        {
            return "watch";
        }
        if (score < 50.5)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 59436.0, in order.</summary>
    public static int AdjustmentReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59436.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AdjustmentReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adjustment reservation code in its canonical AR- form.</summary>
    public static string NormalizeAdjustmentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
