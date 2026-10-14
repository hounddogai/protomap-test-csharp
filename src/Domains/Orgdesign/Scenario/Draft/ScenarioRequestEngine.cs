namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scenario request calculation.</summary>
public interface IScenarioRequestModifier
{
    string Describe();
    double Adjust(ScenarioRequest item, double valueValue);
}

public sealed record PercentScenarioRequestAdjuster(double Percent) : IScenarioRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScenarioRequestDurationMinutesOffsetAdjuster(double Weight) : IScenarioRequestModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record TieredScenarioRequestAdjuster(List<double> Bounds, List<double> Rates) : IScenarioRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
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

public sealed record MinimumScenarioRequestAdjuster(double Minimum) : IScenarioRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScenarioRequestAdjuster(Registry Flags, string Flag, double Percent) : IScenarioRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelScenarioRequestAdjuster(Dictionary<string, double> Factors) : IScenarioRequestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScenarioRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scenario request steps in order and keeps a trace of each.</summary>
public sealed class ScenarioRequestEngine(List<IScenarioRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioRequestEngine DefaultScenarioRequestEngine(Registry flags)
    {
        return new ScenarioRequestEngine(
            new List<IScenarioRequestModifier>
            {
                new PercentScenarioRequestAdjuster(21.0),
                new ScenarioRequestDurationMinutesOffsetAdjuster(0.47),
                new TieredScenarioRequestAdjuster(
                    new List<double> { 1662.0, 4835.0, 6814.0 },
                    new List<double> { 1.07, 0.93, 0.9 }),
                new MinimumScenarioRequestAdjuster(9.0),
                new FlaggedScenarioRequestAdjuster(flags, "scenario-request-regional", 6.8),
                new LabelScenarioRequestAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["east"] = 0.89 }),
            },
            new List<string>(),
            0.3,
            2339.5);
    }

    public double Compute(ScenarioRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
