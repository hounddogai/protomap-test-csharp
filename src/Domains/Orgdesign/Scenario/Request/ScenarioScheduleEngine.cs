namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario schedule calculation.</summary>
public interface IScenarioScheduleFactor
{
    string Describe();
    double Adjust(ScenarioSchedule item, double valueValue);
}

public sealed record PercentScenarioScheduleAdjuster(double Percent) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScenarioScheduleAdjuster(List<double> Bounds, List<double> Rates) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
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

public sealed record CapScenarioScheduleAdjuster(double Cap) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScenarioScheduleAdjuster(double Minimum) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScenarioScheduleAdjuster(double Step) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScenarioScheduleAdjuster(Registry Flags, string Flag, double Percent) : IScenarioScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario schedule steps in order and keeps a trace of each.</summary>
public sealed class ScenarioScheduleEngine(List<IScenarioScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioScheduleEngine DefaultScenarioScheduleEngine(Registry flags)
    {
        return new ScenarioScheduleEngine(
            new List<IScenarioScheduleFactor>
            {
                new PercentScenarioScheduleAdjuster(-12.8),
                new TieredScenarioScheduleAdjuster(
                    new List<double> { 391.0, 4681.0 },
                    new List<double> { 1.03, 1.14 }),
                new CapScenarioScheduleAdjuster(12470.0),
                new MinimumScenarioScheduleAdjuster(39.3),
                new RoundingScenarioScheduleAdjuster(0.5),
                new FlaggedScenarioScheduleAdjuster(flags, "scenario-schedule-default", 8.7),
            },
            new List<string>(),
            1.2,
            51832.9);
    }

    public double Run(ScenarioSchedule item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
