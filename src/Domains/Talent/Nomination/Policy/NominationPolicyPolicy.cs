namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination policy policy.</summary>
public static class NominationPolicyPolicy
{
    /// <summary>Puts a nomination policy code in its canonical NP- form.</summary>
    public static string NormalizeNominationPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string NominationPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
