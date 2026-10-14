namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception checkpoint calculation.</summary>
public interface IExceptionCheckpointStep
{
    string Describe();
    double Adjust(ExceptionCheckpoint item, double valueValue);
}

public sealed record FlaggedExceptionCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IExceptionCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentExceptionCheckpointAdjuster(double Percent) : IExceptionCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapExceptionCheckpointAdjuster(double Cap) : IExceptionCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredExceptionCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IExceptionCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionCheckpoint item, double valueValue)
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

public sealed record RoundingExceptionCheckpointAdjuster(double Step) : IExceptionCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the exception checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ExceptionCheckpointEngine(List<IExceptionCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionCheckpointEngine DefaultExceptionCheckpointEngine(Registry flags)
    {
        return new ExceptionCheckpointEngine(
            new List<IExceptionCheckpointStep>
            {
                new FlaggedExceptionCheckpointAdjuster(flags, "exception-checkpoint-legacy", 5.2),
                new PercentExceptionCheckpointAdjuster(22.6),
                new CapExceptionCheckpointAdjuster(38820.0),
                new TieredExceptionCheckpointAdjuster(
                    new List<double> { 1015.0, 2518.0 },
                    new List<double> { 0.92, 0.88 }),
                new RoundingExceptionCheckpointAdjuster(0.25),
            },
            new List<string>(),
            2.1,
            40300.0);
    }

    public double Run(ExceptionCheckpoint item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
