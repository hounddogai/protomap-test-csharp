namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered policy ack variance policy.</summary>
public static class TieredPolicyAckVariancePolicy
{
    /// <summary>Puts a tiered policy ack variance code in its canonical TPAV- form.</summary>
    public static string NormalizeTieredPolicyAckVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPAV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPAV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredPolicyAckVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
