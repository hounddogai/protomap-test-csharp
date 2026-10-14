namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive offboarding ledger policy.</summary>
public static class AdaptiveOffboardingLedgerPolicy
{
    public static string AdaptiveOffboardingLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveOffboardingLedger()
    {
        var policy = Retry.DefaultPolicy("adaptive-offboarding-ledger");
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
}
