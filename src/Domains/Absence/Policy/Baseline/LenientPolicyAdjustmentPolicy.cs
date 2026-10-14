namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient policy adjustment policy.</summary>
public static class LenientPolicyAdjustmentPolicy
{
    /// <summary>Puts a lenient policy adjustment code in its canonical LPA- form.</summary>
    public static string NormalizeLenientPolicyAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientPolicyAdjustment()
    {
        var policy = Retry.DefaultPolicy("lenient-policy-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 98808.0, in order.</summary>
    public static int LenientPolicyAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98808.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LenientPolicyAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
