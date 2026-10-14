namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback laptop template policy.</summary>
public static class FallbackLaptopTemplatePolicy
{
    /// <summary>Names the band a fallback laptop template score falls in.</summary>
    public static string ClassifyFallbackLaptopTemplate(double score)
    {
        if (score < 25.7)
        {
            return "minimal";
        }
        if (score < 86.6)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackLaptopTemplate()
    {
        var policy = Retry.DefaultPolicy("fallback-laptop-template");
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

    public static List<string> MergeFallbackLaptopTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
