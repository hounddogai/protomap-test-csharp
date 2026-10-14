namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of certification group policy.</summary>
public static class CertificationGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationGroup()
    {
        var policy = Retry.DefaultPolicy("certification-group");
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

    /// <summary>Names the band a certification group score falls in.</summary>
    public static string ClassifyCertificationGroup(double score)
    {
        if (score < 1.1)
        {
            return "high";
        }
        if (score < 37.4)
        {
            return "critical";
        }
        if (score < 63.9)
        {
            return "minimal";
        }
        if (score < 85.8)
        {
            return "medium";
        }
        return "watch";
    }
}
