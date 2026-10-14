namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched pay band template policy.</summary>
public static class BatchedPayBandTemplatePolicy
{
    public static string BatchedPayBandTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int BatchedPayBandTemplateSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a batched pay band template code in its canonical BPBT- form.</summary>
    public static string NormalizeBatchedPayBandTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a batched pay band template score falls in.</summary>
    public static string ClassifyBatchedPayBandTemplate(double score)
    {
        if (score < 23.7)
        {
            return "critical";
        }
        if (score < 28.5)
        {
            return "medium";
        }
        if (score < 99.9)
        {
            return "watch";
        }
        return "elevated";
    }
}
