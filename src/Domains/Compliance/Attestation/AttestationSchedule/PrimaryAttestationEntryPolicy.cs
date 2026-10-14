namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary attestation entry policy.</summary>
public static class PrimaryAttestationEntryPolicy
{
    public static string PrimaryAttestationEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int PrimaryAttestationEntrySpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a primary attestation entry score falls in.</summary>
    public static string ClassifyPrimaryAttestationEntry(double score)
    {
        if (score < 36.8)
        {
            return "elevated";
        }
        if (score < 62.3)
        {
            return "ok";
        }
        return "low";
    }
}
