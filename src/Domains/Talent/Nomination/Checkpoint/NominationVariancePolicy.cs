namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination variance policy.</summary>
public static class NominationVariancePolicy
{
    public static string NominationVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a nomination variance code in its canonical NV- form.</summary>
    public static string NormalizeNominationVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double NominationVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int NominationVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
