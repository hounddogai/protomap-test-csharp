namespace Acme.Workforce.Assets.Assignment.Entry;

/// <summary>One step of the strict assignment checkpoint calculation.</summary>
public interface IStrictAssignmentCheckpointStep
{
    string Describe();
    double Adjust(StrictAssignmentCheckpoint item, double valueValue);
}

public sealed record MinimumStrictAssignmentCheckpointAdjuster(double Minimum) : IStrictAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAssignmentCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictAssignmentCheckpointGraceDaysOffsetAdjuster(double Weight) : IStrictAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(StrictAssignmentCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentStrictAssignmentCheckpointAdjuster(double Percent) : IStrictAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictAssignmentCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictAssignmentCheckpointAdjuster(double Cap) : IStrictAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAssignmentCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelStrictAssignmentCheckpointAdjuster(Dictionary<string, double> Factors) : IStrictAssignmentCheckpointStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictAssignmentCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict assignment checkpoint steps in order and keeps a trace of each.</summary>
public sealed class StrictAssignmentCheckpointEngine(List<IStrictAssignmentCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssignmentCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssignmentCheckpointEngine DefaultStrictAssignmentCheckpointEngine()
    {
        return new StrictAssignmentCheckpointEngine(
            new List<IStrictAssignmentCheckpointStep>
            {
                new MinimumStrictAssignmentCheckpointAdjuster(5.8),
                new StrictAssignmentCheckpointGraceDaysOffsetAdjuster(1.41),
                new PercentStrictAssignmentCheckpointAdjuster(-24.2),
                new CapStrictAssignmentCheckpointAdjuster(8559.0),
                new LabelStrictAssignmentCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.21, ["west"] = 0.85 }),
            },
            new List<string>(),
            2.6,
            37671.1);
    }

    public double Evaluate(StrictAssignmentCheckpoint item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssignmentCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
