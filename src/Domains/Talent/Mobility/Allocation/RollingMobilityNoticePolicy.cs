namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling mobility notice policy.</summary>
public static class RollingMobilityNoticePolicy
{
    public static string RollingMobilityNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a rolling mobility notice code in its canonical RMN- form.</summary>
    public static string NormalizeRollingMobilityNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RMN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RMN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
