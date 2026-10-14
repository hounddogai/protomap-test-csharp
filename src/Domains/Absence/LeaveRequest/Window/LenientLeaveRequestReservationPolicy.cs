namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient leave request reservation policy.</summary>
public static class LenientLeaveRequestReservationPolicy
{
    public static string LenientLeaveRequestReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a lenient leave request reservation score falls in.</summary>
    public static string ClassifyLenientLeaveRequestReservation(double score)
    {
        if (score < 7.5)
        {
            return "watch";
        }
        if (score < 13.2)
        {
            return "elevated";
        }
        if (score < 32.8)
        {
            return "critical";
        }
        if (score < 77.5)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Puts a lenient leave request reservation code in its canonical LLRR- form.</summary>
    public static string NormalizeLenientLeaveRequestReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
