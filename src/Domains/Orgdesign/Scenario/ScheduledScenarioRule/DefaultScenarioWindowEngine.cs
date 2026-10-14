namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default scenario window calculation.</summary>
public interface IDefaultScenarioWindowFactor
{
    string Describe();
    double Adjust(DefaultScenarioWindow item, double valueValue);
}

public sealed record MinimumDefaultScenarioWindowAdjuster(double Minimum) : IDefaultScenarioWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultScenarioWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultScenarioWindowAdjuster(Registry Flags, string Flag, double Percent) : IDefaultScenarioWindowFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultScenarioWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultScenarioWindowAdjuster(double Step) : IDefaultScenarioWindowFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultScenarioWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default scenario window steps in order and keeps a trace of each.</summary>
public sealed class DefaultScenarioWindowEngine(List<IDefaultScenarioWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultScenarioWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultScenarioWindowEngine DefaultDefaultScenarioWindowEngine(Registry flags)
    {
        return new DefaultScenarioWindowEngine(
            new List<IDefaultScenarioWindowFactor>
            {
                new MinimumDefaultScenarioWindowAdjuster(4.5),
                new FlaggedDefaultScenarioWindowAdjuster(flags, "default-scenario-window-pending", 1.8),
                new RoundingDefaultScenarioWindowAdjuster(0.05),
            },
            new List<string>(),
            3.1,
            60668.3);
    }

    public double Evaluate(DefaultScenarioWindow item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultScenarioWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
