namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback license seat ledger policy.</summary>
public static class FallbackLicenseSeatLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackLicenseSeatLedger()
    {
        var policy = Retry.DefaultPolicy("fallback-license-seat-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeFallbackLicenseSeatLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback license seat ledger score falls in.</summary>
    public static string ClassifyFallbackLicenseSeatLedger(double score)
    {
        if (score < 33.4)
        {
            return "watch";
        }
        if (score < 70.7)
        {
            return "low";
        }
        if (score < 73.6)
        {
            return "ok";
        }
        if (score < 95.1)
        {
            return "elevated";
        }
        return "high";
    }

    public static string FallbackLicenseSeatLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
