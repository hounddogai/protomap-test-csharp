namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario threshold calculation.</summary>
public interface IScenarioThresholdFactor
{
    string Describe();
    double Adjust(ScenarioThreshold item, double valueValue);
}

public sealed record FlaggedScenarioThresholdAdjuster(Registry Flags, string Flag, double Percent) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScenarioThresholdWindowDaysOffsetAdjuster(double Weight) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredScenarioThresholdAdjuster(List<double> Bounds, List<double> Rates) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
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

public sealed record RoundingScenarioThresholdAdjuster(double Step) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScenarioThresholdAdjuster(double Percent) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScenarioThresholdAdjuster(double Minimum) : IScenarioThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scenario threshold steps in order and keeps a trace of each.</summary>
public sealed class ScenarioThresholdEngine(List<IScenarioThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioThresholdEngine DefaultScenarioThresholdEngine(Registry flags)
    {
        return new ScenarioThresholdEngine(
            new List<IScenarioThresholdFactor>
            {
                new FlaggedScenarioThresholdAdjuster(flags, "scenario-threshold-legacy", 6.2),
                new ScenarioThresholdWindowDaysOffsetAdjuster(1.45),
                new TieredScenarioThresholdAdjuster(
                    new List<double> { 4420.0, 5044.0, 7260.0 },
                    new List<double> { 0.89, 1.18, 0.92 }),
                new RoundingScenarioThresholdAdjuster(1.0),
                new PercentScenarioThresholdAdjuster(-21.5),
                new MinimumScenarioThresholdAdjuster(36.2),
            },
            new List<string>(),
            4.0,
            22109.3);
    }

    public double Run(ScenarioThreshold item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
