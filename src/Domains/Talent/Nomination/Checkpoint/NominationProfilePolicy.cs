namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination profile policy.</summary>
public static class NominationProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationProfile()
    {
        var policy = Retry.DefaultPolicy("nomination-profile");
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

    public static double NominationProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int NominationProfileSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a nomination profile code in its canonical NP- form.</summary>
    public static string NormalizeNominationProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
