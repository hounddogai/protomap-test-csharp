namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped career path schedule policy.</summary>
public static class CappedCareerPathSchedulePolicy
{
    public static string CappedCareerPathScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a capped career path schedule code in its canonical CCPS- form.</summary>
    public static string NormalizeCappedCareerPathScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CCPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CCPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
