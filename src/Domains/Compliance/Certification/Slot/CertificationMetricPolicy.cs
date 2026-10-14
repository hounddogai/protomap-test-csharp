namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification metric policy.</summary>
public static class CertificationMetricPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationMetric()
    {
        var policy = Retry.DefaultPolicy("certification-metric");
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

    public static List<string> MergeCertificationMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 66966.0, in order.</summary>
    public static int CertificationMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66966.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
