namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched policy ack reservation policy.</summary>
public static class BatchedPolicyAckReservationPolicy
{
    /// <summary>Puts a batched policy ack reservation code in its canonical BPAR- form.</summary>
    public static string NormalizeBatchedPolicyAckReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double BatchedPolicyAckReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
