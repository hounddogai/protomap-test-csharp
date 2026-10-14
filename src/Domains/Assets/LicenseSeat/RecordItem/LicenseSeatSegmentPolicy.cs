namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat segment policy.</summary>
public static class LicenseSeatSegmentPolicy
{
    /// <summary>Puts a license seat segment code in its canonical LSS- form.</summary>
    public static string NormalizeLicenseSeatSegmentCode(string code)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatSegment()
    {
        var policy = Retry.DefaultPolicy("license-seat-segment");
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
}
