namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global policy ack bundle policy.</summary>
public static class GlobalPolicyAckBundlePolicy
{
    public static double GlobalPolicyAckBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a global policy ack bundle code in its canonical GPAB- form.</summary>
    public static string NormalizeGlobalPolicyAckBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
