namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario batch policy.</summary>
public static class ScenarioBatchPolicy
{
    public static string ScenarioBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a scenario batch score falls in.</summary>
    public static string ClassifyScenarioBatch(double score)
    {
        if (score < 45.3)
        {
            return "watch";
        }
        if (score < 66.5)
        {
            return "minimal";
        }
        if (score < 75.4)
        {
            return "low";
        }
        return "critical";
    }
}
