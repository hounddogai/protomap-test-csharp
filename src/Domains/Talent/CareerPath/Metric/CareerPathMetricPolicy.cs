namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path metric policy.</summary>
public static class CareerPathMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 38662.0, in order.</summary>
    public static int CareerPathMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38662.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a career path metric score falls in.</summary>
    public static string ClassifyCareerPathMetric(double score)
    {
        if (score < 48.1)
        {
            return "elevated";
        }
        if (score < 60.0)
        {
            return "high";
        }
        if (score < 82.5)
        {
            return "medium";
        }
        return "minimal";
    }

    public static List<string> MergeCareerPathMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathMetric()
    {
        var policy = Retry.DefaultPolicy("career-path-metric");
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
