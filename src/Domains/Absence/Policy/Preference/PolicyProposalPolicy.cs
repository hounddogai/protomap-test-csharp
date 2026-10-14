namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy proposal policy.</summary>
public static class PolicyProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyProposal()
    {
        var policy = Retry.DefaultPolicy("policy-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a policy proposal score falls in.</summary>
    public static string ClassifyPolicyProposal(double score)
    {
        if (score < 78.8)
        {
            return "medium";
        }
        if (score < 84.0)
        {
            return "minimal";
        }
        return "ok";
    }
}
