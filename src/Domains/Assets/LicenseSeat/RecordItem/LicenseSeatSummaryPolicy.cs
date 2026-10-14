namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat summary policy.</summary>
public static class LicenseSeatSummaryPolicy
{
    /// <summary>Puts a license seat summary code in its canonical LSS- form.</summary>
    public static string NormalizeLicenseSeatSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LicenseSeatSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatSummary()
    {
        var policy = Retry.DefaultPolicy("license-seat-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
