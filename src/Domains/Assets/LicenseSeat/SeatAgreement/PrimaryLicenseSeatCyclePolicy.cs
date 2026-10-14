namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary license seat cycle policy.</summary>
public static class PrimaryLicenseSeatCyclePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryLicenseSeatCycle()
    {
        var policy = Retry.DefaultPolicy("primary-license-seat-cycle");
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

    public static string PrimaryLicenseSeatCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a primary license seat cycle code in its canonical PLSC- form.</summary>
    public static string NormalizePrimaryLicenseSeatCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLSC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLSC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimaryLicenseSeatCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
