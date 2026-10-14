namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient policy ack cycle policy.</summary>
public static class LenientPolicyAckCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37551.0, in order.</summary>
    public static int LenientPolicyAckCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37551.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient policy ack cycle code in its canonical LPAC- form.</summary>
    public static string NormalizeLenientPolicyAckCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LenientPolicyAckCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
