namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work digest policy.</summary>
public static class StatementOfWorkDigestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkDigest()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-digest");
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

    public static string StatementOfWorkDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a statement of work digest score falls in.</summary>
    public static string ClassifyStatementOfWorkDigest(double score)
    {
        if (score < 1.0)
        {
            return "ok";
        }
        if (score < 25.1)
        {
            return "critical";
        }
        return "high";
    }
}
