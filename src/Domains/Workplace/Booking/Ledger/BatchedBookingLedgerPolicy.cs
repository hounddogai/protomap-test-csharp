namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched booking ledger policy.</summary>
public static class BatchedBookingLedgerPolicy
{
    public static double BatchedBookingLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a batched booking ledger score falls in.</summary>
    public static string ClassifyBatchedBookingLedger(double score)
    {
        if (score < 5.1)
        {
            return "high";
        }
        if (score < 94.0)
        {
            return "minimal";
        }
        if (score < 97.6)
        {
            return "ok";
        }
        return "critical";
    }

    public static List<string> MergeBatchedBookingLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
