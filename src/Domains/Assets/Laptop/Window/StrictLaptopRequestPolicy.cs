namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict laptop request policy.</summary>
public static class StrictLaptopRequestPolicy
{
    public static List<string> MergeStrictLaptopRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict laptop request score falls in.</summary>
    public static string ClassifyStrictLaptopRequest(double score)
    {
        if (score < 10.0)
        {
            return "low";
        }
        if (score < 20.3)
        {
            return "minimal";
        }
        if (score < 27.1)
        {
            return "elevated";
        }
        if (score < 39.8)
        {
            return "medium";
        }
        return "critical";
    }

    public static double StrictLaptopRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictLaptopRequest()
    {
        var policy = Retry.DefaultPolicy("strict-laptop-request");
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
