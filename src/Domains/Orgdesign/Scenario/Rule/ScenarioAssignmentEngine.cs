namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scenario assignment calculation.</summary>
public interface IScenarioAssignmentStep
{
    string Describe();
    double Adjust(ScenarioAssignment item, double valueValue);
}

public sealed record ScenarioAssignmentFloorValueOffsetAdjuster(double Weight) : IScenarioAssignmentStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ScenarioAssignment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredScenarioAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IScenarioAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioAssignment item, double valueValue)
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

public sealed record LabelScenarioAssignmentAdjuster(Dictionary<string, double> Factors) : IScenarioAssignmentStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScenarioAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScenarioAssignmentAdjuster(double Percent) : IScenarioAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScenarioAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IScenarioAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario assignment steps in order and keeps a trace of each.</summary>
public sealed class ScenarioAssignmentEngine(List<IScenarioAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioAssignmentEngine DefaultScenarioAssignmentEngine(Registry flags)
    {
        return new ScenarioAssignmentEngine(
            new List<IScenarioAssignmentStep>
            {
                new ScenarioAssignmentFloorValueOffsetAdjuster(0.18),
                new TieredScenarioAssignmentAdjuster(
                    new List<double> { 2972.0, 4002.0, 6382.0, 9763.0 },
                    new List<double> { 0.87, 1.13, 0.8, 1.17 }),
                new LabelScenarioAssignmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.3, ["east"] = 0.77, ["central"] = 1.04 }),
                new PercentScenarioAssignmentAdjuster(11.0),
                new FlaggedScenarioAssignmentAdjuster(flags, "scenario-assignment-manual", -5.5),
            },
            new List<string>(),
            3.4,
            55277.5);
    }

    public double Price(ScenarioAssignment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
