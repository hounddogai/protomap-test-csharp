namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario exception calculation.</summary>
public interface IScenarioExceptionAdjuster
{
    string Describe();
    double Adjust(ScenarioException item, double valueValue);
}

public sealed record RoundingScenarioExceptionAdjuster(double Step) : IScenarioExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScenarioExceptionAdjuster(double Minimum) : IScenarioExceptionAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScenarioExceptionAdjuster(double Percent) : IScenarioExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScenarioExceptionAdjuster(Registry Flags, string Flag, double Percent) : IScenarioExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScenarioExceptionAdjuster(double Cap) : IScenarioExceptionAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scenario exception steps in order and keeps a trace of each.</summary>
public sealed class ScenarioExceptionEngine(List<IScenarioExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioExceptionEngine DefaultScenarioExceptionEngine(Registry flags)
    {
        return new ScenarioExceptionEngine(
            new List<IScenarioExceptionAdjuster>
            {
                new RoundingScenarioExceptionAdjuster(0.05),
                new MinimumScenarioExceptionAdjuster(24.9),
                new PercentScenarioExceptionAdjuster(22.7),
                new FlaggedScenarioExceptionAdjuster(flags, "scenario-exception-default", 3.1),
                new CapScenarioExceptionAdjuster(4850.0),
            },
            new List<string>(),
            3.1,
            57415.3);
    }

    public double Price(ScenarioException item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
