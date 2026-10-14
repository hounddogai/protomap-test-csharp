namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool assignment policy.</summary>
public static class BonusPoolAssignmentPolicy
{
    /// <summary>Puts a bonus pool assignment code in its canonical BPA- form.</summary>
    public static string NormalizeBonusPoolAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BonusPoolAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int BonusPoolAssignmentSpan(int start, int end)
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

    /// <summary>Names the band a bonus pool assignment score falls in.</summary>
    public static string ClassifyBonusPoolAssignment(double score)
    {
        if (score < 46.5)
        {
            return "medium";
        }
        if (score < 99.9)
        {
            return "minimal";
        }
        return "watch";
    }
}
