namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification batch policy.</summary>
public static class CertificationBatchPolicy
{
    public static string CertificationBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double CertificationBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CertificationBatchSpan(int start, int end)
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

    /// <summary>Puts a certification batch code in its canonical CB- form.</summary>
    public static string NormalizeCertificationBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
