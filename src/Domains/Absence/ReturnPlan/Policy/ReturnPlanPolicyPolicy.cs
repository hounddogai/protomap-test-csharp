namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan policy policy.</summary>
public static class ReturnPlanPolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReturnPlanPolicy()
    {
        var policy = Retry.DefaultPolicy("return-plan-policy");
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

    /// <summary>Puts a return plan policy code in its canonical RPP- form.</summary>
    public static string NormalizeReturnPlanPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeReturnPlanPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
