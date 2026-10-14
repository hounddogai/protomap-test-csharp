namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile slot policy.</summary>
public static class SkillProfileSlotPolicy
{
    public static int SkillProfileSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a skill profile slot code in its canonical SPS- form.</summary>
    public static string NormalizeSkillProfileSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SkillProfileSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double SkillProfileSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
