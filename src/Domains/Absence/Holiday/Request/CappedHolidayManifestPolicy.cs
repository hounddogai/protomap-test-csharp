namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped holiday manifest policy.</summary>
public static class CappedHolidayManifestPolicy
{
    /// <summary>Puts a capped holiday manifest code in its canonical CHM- form.</summary>
    public static string NormalizeCappedHolidayManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CHM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CHM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedHolidayManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static string CappedHolidayManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedHolidayManifest()
    {
        var policy = Retry.DefaultPolicy("capped-holiday-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
