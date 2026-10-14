namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered mobility reservation policy.</summary>
public static class TieredMobilityReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 81605.0, in order.</summary>
    public static int TieredMobilityReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81605.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTieredMobilityReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered mobility reservation score falls in.</summary>
    public static string ClassifyTieredMobilityReservation(double score)
    {
        if (score < 34.6)
        {
            return "elevated";
        }
        if (score < 59.2)
        {
            return "medium";
        }
        if (score < 89.4)
        {
            return "critical";
        }
        return "low";
    }
}
