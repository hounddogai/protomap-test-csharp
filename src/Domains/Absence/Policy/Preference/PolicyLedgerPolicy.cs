namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ledger policy.</summary>
public static class PolicyLedgerPolicy
{
    /// <summary>Names the band a policy ledger score falls in.</summary>
    public static string ClassifyPolicyLedger(double score)
    {
        if (score < 32.4)
        {
            return "low";
        }
        if (score < 81.0)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyLedger()
    {
        var policy = Retry.DefaultPolicy("policy-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergePolicyLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
