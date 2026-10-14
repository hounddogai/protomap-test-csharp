namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped benchmark checkpoint calculation.</summary>
public interface ICappedBenchmarkCheckpointModifier
{
    string Describe();
    double Adjust(CappedBenchmarkCheckpoint item, double valueValue);
}

public sealed record FlaggedCappedBenchmarkCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICappedBenchmarkCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedBenchmarkCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedBenchmarkCheckpointAdjuster(double Step) : ICappedBenchmarkCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedBenchmarkCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedBenchmarkCheckpointAdjuster(double Percent) : ICappedBenchmarkCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedBenchmarkCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedBenchmarkCheckpointDiscountOffsetAdjuster(double Weight) : ICappedBenchmarkCheckpointModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(CappedBenchmarkCheckpoint item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapCappedBenchmarkCheckpointAdjuster(double Cap) : ICappedBenchmarkCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBenchmarkCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped benchmark checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CappedBenchmarkCheckpointEngine(List<ICappedBenchmarkCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBenchmarkCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBenchmarkCheckpointEngine DefaultCappedBenchmarkCheckpointEngine(Registry flags)
    {
        return new CappedBenchmarkCheckpointEngine(
            new List<ICappedBenchmarkCheckpointModifier>
            {
                new FlaggedCappedBenchmarkCheckpointAdjuster(flags, "capped-benchmark-checkpoint-primary", -1.2),
                new RoundingCappedBenchmarkCheckpointAdjuster(0.5),
                new PercentCappedBenchmarkCheckpointAdjuster(22.5),
                new CappedBenchmarkCheckpointDiscountOffsetAdjuster(1.62),
                new CapCappedBenchmarkCheckpointAdjuster(38892.0),
            },
            new List<string>(),
            4.4,
            69618.6);
    }

    public double Evaluate(CappedBenchmarkCheckpoint item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedBenchmarkCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
