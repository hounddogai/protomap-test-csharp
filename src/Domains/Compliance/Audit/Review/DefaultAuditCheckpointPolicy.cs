namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default audit checkpoint policy.</summary>
public static class DefaultAuditCheckpointPolicy
{
    /// <summary>Names the band a default audit checkpoint score falls in.</summary>
    public static string ClassifyDefaultAuditCheckpoint(double score)
    {
        if (score < 10.6)
        {
            return "medium";
        }
        if (score < 12.5)
        {
            return "high";
        }
        if (score < 38.6)
        {
            return "elevated";
        }
        if (score < 60.2)
        {
            return "critical";
        }
        return "watch";
    }

    public static List<string> MergeDefaultAuditCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultAuditCheckpoint()
    {
        var policy = Retry.DefaultPolicy("default-audit-checkpoint");
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

    /// <summary>Counts how many amounts fit in a budget of 18825.0, in order.</summary>
    public static int DefaultAuditCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18825.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
