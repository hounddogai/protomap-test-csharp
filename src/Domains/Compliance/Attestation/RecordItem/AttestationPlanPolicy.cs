namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation plan policy.</summary>
public static class AttestationPlanPolicy
{
    public static double AttestationPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AttestationPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
