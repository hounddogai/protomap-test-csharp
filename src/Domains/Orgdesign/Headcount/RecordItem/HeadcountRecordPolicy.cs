namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount record policy.</summary>
public static class HeadcountRecordPolicy
{
    public static double HeadcountRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int HeadcountRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a headcount record code in its canonical HR- form.</summary>
    public static string NormalizeHeadcountRecordCode(string code)
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

    public static string HeadcountRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
