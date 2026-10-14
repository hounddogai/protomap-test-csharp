namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario checkpoint calculation.</summary>
public interface IScenarioCheckpointFactor
{
    string Describe();
    double Adjust(ScenarioCheckpoint item, double valueValue);
}

public sealed record MinimumScenarioCheckpointAdjuster(double Minimum) : IScenarioCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScenarioCheckpointAdjuster(double Percent) : IScenarioCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScenarioCheckpointAdjuster(double Step) : IScenarioCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scenario checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ScenarioCheckpointEngine(List<IScenarioCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioCheckpointEngine DefaultScenarioCheckpointEngine()
    {
        return new ScenarioCheckpointEngine(
            new List<IScenarioCheckpointFactor>
            {
                new MinimumScenarioCheckpointAdjuster(16.4),
                new PercentScenarioCheckpointAdjuster(16.3),
                new RoundingScenarioCheckpointAdjuster(0.5),
            },
            new List<string>(),
            4.2,
            98983.3);
    }

    public double Compute(ScenarioCheckpoint item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
