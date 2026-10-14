namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit policy policy.</summary>
public static class AuditPolicyPolicy
{
    public static double AuditPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AuditPolicySpan(int start, int end)
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
