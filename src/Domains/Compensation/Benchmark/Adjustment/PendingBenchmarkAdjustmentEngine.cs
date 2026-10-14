namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>One step of the pending benchmark adjustment calculation.</summary>
public interface IPendingBenchmarkAdjustmentModifier
{
    string Describe();
    double Adjust(PendingBenchmarkAdjustment item, double valueValue);
}

public sealed record MinimumPendingBenchmarkAdjustmentAdjuster(double Minimum) : IPendingBenchmarkAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingBenchmarkAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingBenchmarkAdjustmentAttemptsOffsetAdjuster(double Weight) : IPendingBenchmarkAdjustmentModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(PendingBenchmarkAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapPendingBenchmarkAdjustmentAdjuster(double Cap) : IPendingBenchmarkAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBenchmarkAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelPendingBenchmarkAdjustmentAdjuster(Dictionary<string, double> Factors) : IPendingBenchmarkAdjustmentModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PendingBenchmarkAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending benchmark adjustment steps in order and keeps a trace of each.</summary>
public sealed class PendingBenchmarkAdjustmentEngine(List<IPendingBenchmarkAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBenchmarkAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBenchmarkAdjustmentEngine DefaultPendingBenchmarkAdjustmentEngine()
    {
        return new PendingBenchmarkAdjustmentEngine(
            new List<IPendingBenchmarkAdjustmentModifier>
            {
                new MinimumPendingBenchmarkAdjustmentAdjuster(36.9),
                new PendingBenchmarkAdjustmentAttemptsOffsetAdjuster(1.86),
                new CapPendingBenchmarkAdjustmentAdjuster(29272.0),
                new ChannelPendingBenchmarkAdjustmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.97, ["north"] = 0.98, ["retail"] = 1.14 }),
            },
            new List<string>(),
            3.3,
            58675.7);
    }

    public double Evaluate(PendingBenchmarkAdjustment item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingBenchmarkAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
