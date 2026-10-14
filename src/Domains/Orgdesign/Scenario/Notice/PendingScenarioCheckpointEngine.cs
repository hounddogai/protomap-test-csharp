namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending scenario checkpoint calculation.</summary>
public interface IPendingScenarioCheckpointModifier
{
    string Describe();
    double Adjust(PendingScenarioCheckpoint item, double valueValue);
}

public sealed record FlaggedPendingScenarioCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPendingScenarioCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingScenarioCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryPendingScenarioCheckpointAdjuster(Dictionary<string, double> Factors) : IPendingScenarioCheckpointModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PendingScenarioCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingScenarioCheckpointAdjuster(double Minimum) : IPendingScenarioCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingScenarioCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPendingScenarioCheckpointAdjuster(double Step) : IPendingScenarioCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingScenarioCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending scenario checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PendingScenarioCheckpointEngine(List<IPendingScenarioCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingScenarioCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingScenarioCheckpointEngine DefaultPendingScenarioCheckpointEngine(Registry flags)
    {
        return new PendingScenarioCheckpointEngine(
            new List<IPendingScenarioCheckpointModifier>
            {
                new FlaggedPendingScenarioCheckpointAdjuster(flags, "pending-scenario-checkpoint-rolling", -0.6),
                new CountryPendingScenarioCheckpointAdjuster(new Dictionary<string, double> { ["east"] = 1.39 }),
                new MinimumPendingScenarioCheckpointAdjuster(31.5),
                new RoundingPendingScenarioCheckpointAdjuster(0.5),
            },
            new List<string>(),
            4.8,
            69710.0);
    }

    public double Evaluate(PendingScenarioCheckpoint item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingScenarioCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
