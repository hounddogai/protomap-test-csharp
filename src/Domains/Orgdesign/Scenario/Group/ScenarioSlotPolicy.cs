namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario slot policy.</summary>
public static class ScenarioSlotPolicy
{
    /// <summary>Names the band a scenario slot score falls in.</summary>
    public static string ClassifyScenarioSlot(double score)
    {
        if (score < 14.2)
        {
            return "medium";
        }
        if (score < 50.5)
        {
            return "elevated";
        }
        if (score < 73.9)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 62386.0, in order.</summary>
    public static int ScenarioSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62386.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ScenarioSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double ScenarioSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
