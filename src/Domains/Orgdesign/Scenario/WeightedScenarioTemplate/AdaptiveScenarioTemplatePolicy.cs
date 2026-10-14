namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive scenario template policy.</summary>
public static class AdaptiveScenarioTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveScenarioTemplate()
    {
        var policy = Retry.DefaultPolicy("adaptive-scenario-template");
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

    /// <summary>Names the band a adaptive scenario template score falls in.</summary>
    public static string ClassifyAdaptiveScenarioTemplate(double score)
    {
        if (score < 4.9)
        {
            return "elevated";
        }
        if (score < 16.9)
        {
            return "ok";
        }
        return "high";
    }
}
