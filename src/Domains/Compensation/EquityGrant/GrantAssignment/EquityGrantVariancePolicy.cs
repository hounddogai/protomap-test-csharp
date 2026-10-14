namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of equity grant variance policy.</summary>
public static class EquityGrantVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 90614.0, in order.</summary>
    public static int EquityGrantVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90614.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a equity grant variance score falls in.</summary>
    public static string ClassifyEquityGrantVariance(double score)
    {
        if (score < 9.5)
        {
            return "low";
        }
        if (score < 40.1)
        {
            return "ok";
        }
        if (score < 41.6)
        {
            return "watch";
        }
        if (score < 69.0)
        {
            return "elevated";
        }
        return "medium";
    }

    public static double EquityGrantVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEquityGrantVariance()
    {
        var policy = Retry.DefaultPolicy("equity-grant-variance");
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
