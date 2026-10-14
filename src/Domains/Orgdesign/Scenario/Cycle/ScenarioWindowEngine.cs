namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario window calculation.</summary>
public interface IScenarioWindowStep
{
    string Describe();
    double Adjust(ScenarioWindow item, double valueValue);
}

public sealed record CapScenarioWindowAdjuster(double Cap) : IScenarioWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelScenarioWindowAdjuster(Dictionary<string, double> Factors) : IScenarioWindowStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScenarioWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScenarioWindowAdjuster(double Percent) : IScenarioWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScenarioWindowAdjuster(double Step) : IScenarioWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ScenarioWindowCapacityOffsetAdjuster(double Weight) : IScenarioWindowStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(ScenarioWindow item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the scenario window steps in order and keeps a trace of each.</summary>
public sealed class ScenarioWindowEngine(List<IScenarioWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioWindowEngine DefaultScenarioWindowEngine()
    {
        return new ScenarioWindowEngine(
            new List<IScenarioWindowStep>
            {
                new CapScenarioWindowAdjuster(46292.0),
                new LabelScenarioWindowAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.88, ["central"] = 1.21 }),
                new PercentScenarioWindowAdjuster(-21.3),
                new RoundingScenarioWindowAdjuster(0.1),
                new ScenarioWindowCapacityOffsetAdjuster(1.02),
            },
            new List<string>(),
            4.8,
            75438.7);
    }

    public double Evaluate(ScenarioWindow item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
