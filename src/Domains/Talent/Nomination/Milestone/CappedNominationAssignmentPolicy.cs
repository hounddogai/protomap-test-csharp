namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped nomination assignment policy.</summary>
public static class CappedNominationAssignmentPolicy
{
    /// <summary>Names the band a capped nomination assignment score falls in.</summary>
    public static string ClassifyCappedNominationAssignment(double score)
    {
        if (score < 20.2)
        {
            return "high";
        }
        if (score < 69.6)
        {
            return "watch";
        }
        if (score < 77.9)
        {
            return "ok";
        }
        if (score < 86.8)
        {
            return "low";
        }
        return "medium";
    }

    public static List<string> MergeCappedNominationAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedNominationAssignment()
    {
        var policy = Retry.DefaultPolicy("capped-nomination-assignment");
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
}
