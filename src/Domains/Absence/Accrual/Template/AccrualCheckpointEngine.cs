namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual checkpoint calculation.</summary>
public interface IAccrualCheckpointStep
{
    string Describe();
    double Adjust(AccrualCheckpoint item, double valueValue);
}

public sealed record FlaggedAccrualCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAccrualCheckpointAdjuster(double Percent) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccrualCheckpointMinimumOffsetAdjuster(double Weight) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumAccrualCheckpointAdjuster(double Minimum) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleAccrualCheckpointAdjuster(Dictionary<string, double> Factors) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAccrualCheckpointAdjuster(double Step) : IAccrualCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AccrualCheckpointEngine(List<IAccrualCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualCheckpointEngine DefaultAccrualCheckpointEngine(Registry flags)
    {
        return new AccrualCheckpointEngine(
            new List<IAccrualCheckpointStep>
            {
                new FlaggedAccrualCheckpointAdjuster(flags, "accrual-checkpoint-pending", -7.5),
                new PercentAccrualCheckpointAdjuster(-14.4),
                new AccrualCheckpointMinimumOffsetAdjuster(1.44),
                new MinimumAccrualCheckpointAdjuster(43.0),
                new TitleAccrualCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.84, ["central"] = 1.1, ["north"] = 1.32 }),
                new RoundingAccrualCheckpointAdjuster(5.0),
            },
            new List<string>(),
            1.4,
            24981.5);
    }

    public double Run(AccrualCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
