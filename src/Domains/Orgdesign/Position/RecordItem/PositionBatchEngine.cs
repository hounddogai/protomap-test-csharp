namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position batch calculation.</summary>
public interface IPositionBatchStep
{
    string Describe();
    double Adjust(PositionBatch item, double valueValue);
}

public sealed record PercentPositionBatchAdjuster(double Percent) : IPositionBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPositionBatchAdjuster(double Cap) : IPositionBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPositionBatchAdjuster(double Minimum) : IPositionBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PositionBatchScoreOffsetAdjuster(double Weight) : IPositionBatchStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingPositionBatchAdjuster(double Step) : IPositionBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPositionBatchAdjuster(Registry Flags, string Flag, double Percent) : IPositionBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position batch steps in order and keeps a trace of each.</summary>
public sealed class PositionBatchEngine(List<IPositionBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionBatchEngine DefaultPositionBatchEngine(Registry flags)
    {
        return new PositionBatchEngine(
            new List<IPositionBatchStep>
            {
                new PercentPositionBatchAdjuster(-16.8),
                new CapPositionBatchAdjuster(24507.0),
                new MinimumPositionBatchAdjuster(12.0),
                new PositionBatchScoreOffsetAdjuster(1.02),
                new RoundingPositionBatchAdjuster(0.05),
                new FlaggedPositionBatchAdjuster(flags, "position-batch-global", 8.9),
            },
            new List<string>(),
            1.7,
            87428.7);
    }

    public double Evaluate(PositionBatch item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
