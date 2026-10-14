namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario score calculation.</summary>
public interface IScenarioScoreModifier
{
    string Describe();
    double Adjust(ScenarioScore item, double valueValue);
}

public sealed record ScenarioScoreUpdatedAtOffsetAdjuster(double Weight) : IScenarioScoreModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(ScenarioScore item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record ChannelScenarioScoreAdjuster(Dictionary<string, double> Factors) : IScenarioScoreModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScenarioScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScenarioScoreAdjuster(double Percent) : IScenarioScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScenarioScoreAdjuster(Registry Flags, string Flag, double Percent) : IScenarioScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScenarioScoreAdjuster(double Step) : IScenarioScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scenario score steps in order and keeps a trace of each.</summary>
public sealed class ScenarioScoreEngine(List<IScenarioScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioScoreEngine DefaultScenarioScoreEngine(Registry flags)
    {
        return new ScenarioScoreEngine(
            new List<IScenarioScoreModifier>
            {
                new ScenarioScoreUpdatedAtOffsetAdjuster(0.34),
                new ChannelScenarioScoreAdjuster(new Dictionary<string, double> { ["retail"] = 0.86 }),
                new PercentScenarioScoreAdjuster(-1.4),
                new FlaggedScenarioScoreAdjuster(flags, "scenario-score-lenient", 0.3),
                new RoundingScenarioScoreAdjuster(0.05),
            },
            new List<string>(),
            2.6,
            80494.2);
    }

    public double Run(ScenarioScore item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
