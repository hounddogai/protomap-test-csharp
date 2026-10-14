namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy license seat window policy.</summary>
public static class LegacyLicenseSeatWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyLicenseSeatWindow()
    {
        var policy = Retry.DefaultPolicy("legacy-license-seat-window");
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

    /// <summary>Puts a legacy license seat window code in its canonical LLSW- form.</summary>
    public static string NormalizeLegacyLicenseSeatWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLSW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLSW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyLicenseSeatWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
