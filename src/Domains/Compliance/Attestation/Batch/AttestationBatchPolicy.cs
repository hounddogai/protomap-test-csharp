namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation batch policy.</summary>
public static class AttestationBatchPolicy
{
    public static int AttestationBatchSpan(int start, int end)
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
        return span / 3600;
    }

    public static double AttestationBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
