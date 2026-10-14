namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of certification snapshot policy.</summary>
public static class CertificationSnapshotPolicy
{
    /// <summary>Names the band a certification snapshot score falls in.</summary>
    public static string ClassifyCertificationSnapshot(double score)
    {
        if (score < 26.7)
        {
            return "medium";
        }
        if (score < 72.4)
        {
            return "ok";
        }
        if (score < 95.9)
        {
            return "low";
        }
        return "minimal";
    }

    public static double CertificationSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationSnapshot()
    {
        var policy = Retry.DefaultPolicy("certification-snapshot");
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
