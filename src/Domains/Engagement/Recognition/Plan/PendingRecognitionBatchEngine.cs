namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending recognition batch calculation.</summary>
public interface IPendingRecognitionBatchModifier
{
    string Describe();
    double Adjust(PendingRecognitionBatch item, double valueValue);
}

public sealed record PendingRecognitionBatchDurationMinutesOffsetAdjuster(double Weight) : IPendingRecognitionBatchModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PendingRecognitionBatch item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record FlaggedPendingRecognitionBatchAdjuster(Registry Flags, string Flag, double Percent) : IPendingRecognitionBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingRecognitionBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingRecognitionBatchAdjuster(double Minimum) : IPendingRecognitionBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingRecognitionBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingRecognitionBatchAdjuster(double Cap) : IPendingRecognitionBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingRecognitionBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingRecognitionBatchAdjuster(double Step) : IPendingRecognitionBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingRecognitionBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending recognition batch steps in order and keeps a trace of each.</summary>
public sealed class PendingRecognitionBatchEngine(List<IPendingRecognitionBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingRecognitionBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingRecognitionBatchEngine DefaultPendingRecognitionBatchEngine(Registry flags)
    {
        return new PendingRecognitionBatchEngine(
            new List<IPendingRecognitionBatchModifier>
            {
                new PendingRecognitionBatchDurationMinutesOffsetAdjuster(0.86),
                new FlaggedPendingRecognitionBatchAdjuster(flags, "pending-recognition-batch-lenient", 8.6),
                new MinimumPendingRecognitionBatchAdjuster(10.4),
                new CapPendingRecognitionBatchAdjuster(47204.0),
                new RoundingPendingRecognitionBatchAdjuster(0.05),
            },
            new List<string>(),
            1.9,
            32839.8);
    }

    public double Run(PendingRecognitionBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingRecognitionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
