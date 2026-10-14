namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy notice policy.</summary>
public static class OccupancyNoticePolicy
{
    /// <summary>Puts a occupancy notice code in its canonical ON- form.</summary>
    public static string NormalizeOccupancyNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ON-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ON-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string OccupancyNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
