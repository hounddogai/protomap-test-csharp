namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default attestation allocation policy.</summary>
public static class DefaultAttestationAllocationPolicy
{
    public static int DefaultAttestationAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double DefaultAttestationAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
