namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification quota policy.</summary>
public static class CertificationQuotaPolicy
{
    public static string CertificationQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int CertificationQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
