namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted audit record policy.</summary>
public static class WeightedAuditRecordPolicy
{
    /// <summary>Puts a weighted audit record code in its canonical WAR- form.</summary>
    public static string NormalizeWeightedAuditRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted audit record score falls in.</summary>
    public static string ClassifyWeightedAuditRecord(double score)
    {
        if (score < 2.4)
        {
            return "ok";
        }
        if (score < 8.2)
        {
            return "high";
        }
        if (score < 57.4)
        {
            return "minimal";
        }
        return "watch";
    }

    public static string WeightedAuditRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
