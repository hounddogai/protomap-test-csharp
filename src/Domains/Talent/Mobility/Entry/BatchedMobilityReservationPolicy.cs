namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched mobility reservation policy.</summary>
public static class BatchedMobilityReservationPolicy
{
    public static int BatchedMobilityReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a batched mobility reservation score falls in.</summary>
    public static string ClassifyBatchedMobilityReservation(double score)
    {
        if (score < 25.9)
        {
            return "high";
        }
        if (score < 30.3)
        {
            return "elevated";
        }
        if (score < 34.3)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 21876.0, in order.</summary>
    public static int BatchedMobilityReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21876.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBatchedMobilityReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
