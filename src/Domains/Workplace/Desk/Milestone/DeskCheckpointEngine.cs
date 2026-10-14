namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the desk checkpoint calculation.</summary>
public interface IDeskCheckpointStep
{
    string Describe();
    double Adjust(DeskCheckpoint item, double valueValue);
}

public sealed record PercentDeskCheckpointAdjuster(double Percent) : IDeskCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeskCheckpointAdjuster(double Minimum) : IDeskCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDeskCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IDeskCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskCheckpoint item, double valueValue)
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

public sealed record FlaggedDeskCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IDeskCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk checkpoint steps in order and keeps a trace of each.</summary>
public sealed class DeskCheckpointEngine(List<IDeskCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskCheckpointEngine DefaultDeskCheckpointEngine(Registry flags)
    {
        return new DeskCheckpointEngine(
            new List<IDeskCheckpointStep>
            {
                new PercentDeskCheckpointAdjuster(-12.5),
                new MinimumDeskCheckpointAdjuster(15.9),
                new TieredDeskCheckpointAdjuster(
                    new List<double> { 1607.0, 5869.0, 8670.0, 8948.0 },
                    new List<double> { 0.99, 0.88, 1.08, 1.15 }),
                new FlaggedDeskCheckpointAdjuster(flags, "desk-checkpoint-rolling", -1.4),
            },
            new List<string>(),
            1.2,
            27002.6);
    }

    public double Price(DeskCheckpoint item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
