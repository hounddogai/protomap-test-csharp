namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario slot calculation.</summary>
public interface IScenarioSlotAdjuster
{
    string Describe();
    double Adjust(ScenarioSlot item, double valueValue);
}

public sealed record TieredScenarioSlotAdjuster(List<double> Bounds, List<double> Rates) : IScenarioSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioSlot item, double valueValue)
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

public sealed record CapScenarioSlotAdjuster(double Cap) : IScenarioSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScenarioSlotAdjuster(double Step) : IScenarioSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScenarioSlotAdjuster(double Percent) : IScenarioSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScenarioSlotAdjuster(double Minimum) : IScenarioSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scenario slot steps in order and keeps a trace of each.</summary>
public sealed class ScenarioSlotEngine(List<IScenarioSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioSlotEngine DefaultScenarioSlotEngine()
    {
        return new ScenarioSlotEngine(
            new List<IScenarioSlotAdjuster>
            {
                new TieredScenarioSlotAdjuster(new List<double> { 6531.0, 8320.0 }, new List<double> { 1.06, 0.81 }),
                new CapScenarioSlotAdjuster(46226.0),
                new RoundingScenarioSlotAdjuster(0.05),
                new PercentScenarioSlotAdjuster(5.3),
                new MinimumScenarioSlotAdjuster(24.6),
            },
            new List<string>(),
            2.3,
            32434.2);
    }

    public double Evaluate(ScenarioSlot item)
    {
        var current = item.NetVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ScenarioSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
