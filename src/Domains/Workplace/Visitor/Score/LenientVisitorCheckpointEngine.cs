namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient visitor checkpoint calculation.</summary>
public interface ILenientVisitorCheckpointModifier
{
    string Describe();
    double Adjust(LenientVisitorCheckpoint item, double valueValue);
}

public sealed record PercentLenientVisitorCheckpointAdjuster(double Percent) : ILenientVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientVisitorCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientVisitorCheckpointAdjuster(double Minimum) : ILenientVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientVisitorCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientVisitorCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ILenientVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientVisitorCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameLenientVisitorCheckpointAdjuster(Dictionary<string, double> Factors) : ILenientVisitorCheckpointModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientVisitorCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient visitor checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LenientVisitorCheckpointEngine(List<ILenientVisitorCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVisitorCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVisitorCheckpointEngine DefaultLenientVisitorCheckpointEngine(Registry flags)
    {
        return new LenientVisitorCheckpointEngine(
            new List<ILenientVisitorCheckpointModifier>
            {
                new PercentLenientVisitorCheckpointAdjuster(-24.2),
                new MinimumLenientVisitorCheckpointAdjuster(18.0),
                new FlaggedLenientVisitorCheckpointAdjuster(flags, "lenient-visitor-checkpoint-lenient", -2.3),
                new NameLenientVisitorCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.35, ["east"] = 0.75, ["retail"] = 1.11 }),
            },
            new List<string>(),
            2.6,
            6523.4);
    }

    public double Compute(LenientVisitorCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientVisitorCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
