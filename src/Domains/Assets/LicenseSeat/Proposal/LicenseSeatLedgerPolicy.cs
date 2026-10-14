namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat ledger policy.</summary>
public static class LicenseSeatLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatLedger()
    {
        var policy = Retry.DefaultPolicy("license-seat-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string LicenseSeatLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
