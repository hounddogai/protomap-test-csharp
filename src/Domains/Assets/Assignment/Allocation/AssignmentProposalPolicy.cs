namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment proposal policy.</summary>
public static class AssignmentProposalPolicy
{
    /// <summary>Names the band a assignment proposal score falls in.</summary>
    public static string ClassifyAssignmentProposal(double score)
    {
        if (score < 9.2)
        {
            return "elevated";
        }
        if (score < 59.9)
        {
            return "ok";
        }
        if (score < 99.6)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 26554.0, in order.</summary>
    public static int AssignmentProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26554.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAssignmentProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentProposal()
    {
        var policy = Retry.DefaultPolicy("assignment-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
