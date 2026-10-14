namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount reservation policy.</summary>
public static class HeadcountReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountReservation()
    {
        var policy = Retry.DefaultPolicy("headcount-reservation");
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

    /// <summary>Names the band a headcount reservation score falls in.</summary>
    public static string ClassifyHeadcountReservation(double score)
    {
        if (score < 8.9)
        {
            return "high";
        }
        if (score < 34.4)
        {
            return "ok";
        }
        if (score < 38.7)
        {
            return "medium";
        }
        if (score < 72.2)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static string HeadcountReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a headcount reservation code in its canonical HR- form.</summary>
    public static string NormalizeHeadcountReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
