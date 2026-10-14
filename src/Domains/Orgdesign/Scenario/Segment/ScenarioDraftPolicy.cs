namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario draft policy.</summary>
public static class ScenarioDraftPolicy
{
    public static double ScenarioDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scenario draft score falls in.</summary>
    public static string ClassifyScenarioDraft(double score)
    {
        if (score < 57.9)
        {
            return "ok";
        }
        if (score < 69.4)
        {
            return "elevated";
        }
        if (score < 84.1)
        {
            return "low";
        }
        if (score < 92.6)
        {
            return "minimal";
        }
        return "watch";
    }
}
