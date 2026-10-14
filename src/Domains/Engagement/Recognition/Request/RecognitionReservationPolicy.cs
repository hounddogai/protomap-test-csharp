namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition reservation policy.</summary>
public static class RecognitionReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionReservation()
    {
        var policy = Retry.DefaultPolicy("recognition-reservation");
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

    /// <summary>Puts a recognition reservation code in its canonical RR- form.</summary>
    public static string NormalizeRecognitionReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RecognitionReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double RecognitionReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
