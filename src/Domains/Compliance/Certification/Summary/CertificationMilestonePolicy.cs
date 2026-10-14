namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification milestone policy.</summary>
public static class CertificationMilestonePolicy
{
    /// <summary>Names the band a certification milestone score falls in.</summary>
    public static string ClassifyCertificationMilestone(double score)
    {
        if (score < 20.0)
        {
            return "ok";
        }
        if (score < 20.7)
        {
            return "minimal";
        }
        if (score < 22.6)
        {
            return "low";
        }
        return "watch";
    }

    public static List<string> MergeCertificationMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationMilestone()
    {
        var policy = Retry.DefaultPolicy("certification-milestone");
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
}
