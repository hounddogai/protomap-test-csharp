namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted license seat bundle policy.</summary>
public static class WeightedLicenseSeatBundlePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedLicenseSeatBundle()
    {
        var policy = Retry.DefaultPolicy("weighted-license-seat-bundle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a weighted license seat bundle code in its canonical WLSB- form.</summary>
    public static string NormalizeWeightedLicenseSeatBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string WeightedLicenseSeatBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int WeightedLicenseSeatBundleSpan(int start, int end)
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
}
