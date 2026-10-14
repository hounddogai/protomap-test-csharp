namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination window policy.</summary>
public static class NominationWindowPolicy
{
    public static double NominationWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationWindow()
    {
        var policy = Retry.DefaultPolicy("nomination-window");
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

    /// <summary>Puts a nomination window code in its canonical NW- form.</summary>
    public static string NormalizeNominationWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a nomination window score falls in.</summary>
    public static string ClassifyNominationWindow(double score)
    {
        if (score < 16.2)
        {
            return "high";
        }
        if (score < 64.9)
        {
            return "ok";
        }
        return "minimal";
    }
}
