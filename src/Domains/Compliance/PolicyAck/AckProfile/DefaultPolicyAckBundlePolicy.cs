namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default policy ack bundle policy.</summary>
public static class DefaultPolicyAckBundlePolicy
{
    public static double DefaultPolicyAckBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default policy ack bundle code in its canonical DPAB- form.</summary>
    public static string NormalizeDefaultPolicyAckBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
