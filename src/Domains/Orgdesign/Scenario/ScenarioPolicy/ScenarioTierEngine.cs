namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario tier calculation.</summary>
public interface IScenarioTierStep
{
    string Describe();
    double Adjust(ScenarioTier item, double valueValue);
}

public sealed record CapScenarioTierAdjuster(double Cap) : IScenarioTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScenarioTierAdjuster(double Percent) : IScenarioTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitScenarioTierAdjuster(Dictionary<string, double> Factors) : IScenarioTierStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ScenarioTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScenarioTierAdjuster(List<double> Bounds, List<double> Rates) : IScenarioTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioTier item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record ScenarioTierRevisionOffsetAdjuster(double Weight) : IScenarioTierStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(ScenarioTier item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the scenario tier steps in order and keeps a trace of each.</summary>
public sealed class ScenarioTierEngine(List<IScenarioTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioTierEngine DefaultScenarioTierEngine()
    {
        return new ScenarioTierEngine(
            new List<IScenarioTierStep>
            {
                new CapScenarioTierAdjuster(13191.0),
                new PercentScenarioTierAdjuster(-19.3),
                new UnitScenarioTierAdjuster(new Dictionary<string, double> { ["east"] = 1.27, ["north"] = 0.94 }),
                new TieredScenarioTierAdjuster(new List<double> { 5137.0, 7598.0 }, new List<double> { 1.01, 1.14 }),
                new ScenarioTierRevisionOffsetAdjuster(0.89),
            },
            new List<string>(),
            2.8,
            22622.4);
    }

    public double Price(ScenarioTier item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ScenarioTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
