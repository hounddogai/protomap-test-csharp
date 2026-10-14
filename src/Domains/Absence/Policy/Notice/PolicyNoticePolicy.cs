namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy notice policy.</summary>
public static class PolicyNoticePolicy
{
    public static string PolicyNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a policy notice code in its canonical PN- form.</summary>
    public static string NormalizePolicyNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PolicyNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double PolicyNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
