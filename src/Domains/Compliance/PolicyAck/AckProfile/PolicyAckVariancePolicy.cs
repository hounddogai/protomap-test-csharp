namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack variance policy.</summary>
public static class PolicyAckVariancePolicy
{
    /// <summary>Names the band a policy ack variance score falls in.</summary>
    public static string ClassifyPolicyAckVariance(double score)
    {
        if (score < 22.1)
        {
            return "watch";
        }
        if (score < 86.4)
        {
            return "minimal";
        }
        return "ok";
    }

    public static List<string> MergePolicyAckVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckVariance()
    {
        var policy = Retry.DefaultPolicy("policy-ack-variance");
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
}
