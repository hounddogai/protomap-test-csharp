namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit agreement policy.</summary>
public static class AuditAgreementPolicy
{
    public static string AuditAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int AuditAgreementSpan(int start, int end)
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
}
