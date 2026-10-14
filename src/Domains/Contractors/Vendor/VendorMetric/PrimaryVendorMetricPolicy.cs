namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary vendor metric policy.</summary>
public static class PrimaryVendorMetricPolicy
{
    /// <summary>Names the band a primary vendor metric score falls in.</summary>
    public static string ClassifyPrimaryVendorMetric(double score)
    {
        if (score < 73.6)
        {
            return "medium";
        }
        if (score < 98.7)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 56369.0, in order.</summary>
    public static int PrimaryVendorMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56369.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryVendorMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryVendorMetric()
    {
        var policy = Retry.DefaultPolicy("primary-vendor-metric");
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
