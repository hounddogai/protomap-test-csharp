namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario target calculation.</summary>
public interface IScenarioTargetFactor
{
    string Describe();
    double Adjust(ScenarioTarget item, double valueValue);
}

public sealed record CapScenarioTargetAdjuster(double Cap) : IScenarioTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ScenarioTargetWindowDaysOffsetAdjuster(double Weight) : IScenarioTargetFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record RoundingScenarioTargetAdjuster(double Step) : IScenarioTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameScenarioTargetAdjuster(Dictionary<string, double> Factors) : IScenarioTargetFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScenarioTargetAdjuster(List<double> Bounds, List<double> Rates) : IScenarioTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
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

public sealed record PercentScenarioTargetAdjuster(double Percent) : IScenarioTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario target steps in order and keeps a trace of each.</summary>
public sealed class ScenarioTargetEngine(List<IScenarioTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioTargetEngine DefaultScenarioTargetEngine()
    {
        return new ScenarioTargetEngine(
            new List<IScenarioTargetFactor>
            {
                new CapScenarioTargetAdjuster(36783.0),
                new ScenarioTargetWindowDaysOffsetAdjuster(1.36),
                new RoundingScenarioTargetAdjuster(5.0),
                new NameScenarioTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.26, ["central"] = 0.81 }),
                new TieredScenarioTargetAdjuster(
                    new List<double> { 1239.0, 8840.0 },
                    new List<double> { 1.09, 0.83 }),
                new PercentScenarioTargetAdjuster(-18.5),
            },
            new List<string>(),
            0.5,
            9768.0);
    }

    public double Compute(ScenarioTarget item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
