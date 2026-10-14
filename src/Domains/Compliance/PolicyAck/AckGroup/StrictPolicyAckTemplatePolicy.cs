namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict policy ack template policy.</summary>
public static class StrictPolicyAckTemplatePolicy
{
    /// <summary>Puts a strict policy ack template code in its canonical SPAT- form.</summary>
    public static string NormalizeStrictPolicyAckTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StrictPolicyAckTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string StrictPolicyAckTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
