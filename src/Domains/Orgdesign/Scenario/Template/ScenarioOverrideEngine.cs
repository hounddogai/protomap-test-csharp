namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scenario override calculation.</summary>
public interface IScenarioOverrideStep
{
    string Describe();
    double Adjust(ScenarioOverride item, double valueValue);
}

public sealed record CapScenarioOverrideAdjuster(double Cap) : IScenarioOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScenarioOverrideAdjuster(List<double> Bounds, List<double> Rates) : IScenarioOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
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

public sealed record ScenarioOverrideMarginOffsetAdjuster(double Weight) : IScenarioOverrideStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record FlaggedScenarioOverrideAdjuster(Registry Flags, string Flag, double Percent) : IScenarioOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitScenarioOverrideAdjuster(Dictionary<string, double> Factors) : IScenarioOverrideStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScenarioOverrideAdjuster(double Minimum) : IScenarioOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scenario override steps in order and keeps a trace of each.</summary>
public sealed class ScenarioOverrideEngine(List<IScenarioOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioOverrideEngine DefaultScenarioOverrideEngine(Registry flags)
    {
        return new ScenarioOverrideEngine(
            new List<IScenarioOverrideStep>
            {
                new CapScenarioOverrideAdjuster(27202.0),
                new TieredScenarioOverrideAdjuster(
                    new List<double> { 1896.0, 2059.0, 7393.0, 8249.0 },
                    new List<double> { 1.14, 0.92, 0.97, 1.01 }),
                new ScenarioOverrideMarginOffsetAdjuster(1.66),
                new FlaggedScenarioOverrideAdjuster(flags, "scenario-override-lenient", 6.7),
                new UnitScenarioOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.8, ["west"] = 1.09 }),
                new MinimumScenarioOverrideAdjuster(26.9),
            },
            new List<string>(),
            3.9,
            62535.5);
    }

    public double Evaluate(ScenarioOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
