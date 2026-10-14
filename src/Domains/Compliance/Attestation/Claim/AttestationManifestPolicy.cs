namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation manifest policy.</summary>
public static class AttestationManifestPolicy
{
    public static int AttestationManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double AttestationManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
