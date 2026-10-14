namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary policy override policy.</summary>
public static class PrimaryPolicyOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPolicyOverride()
    {
        var policy = Retry.DefaultPolicy("primary-policy-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergePrimaryPolicyOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PrimaryPolicyOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 14905.0, in order.</summary>
    public static int PrimaryPolicyOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14905.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
