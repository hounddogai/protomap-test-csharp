namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched pay band mapping calculation.</summary>
public interface IBatchedPayBandMappingAdjuster
{
    string Describe();
    double Adjust(BatchedPayBandMapping item, double valueValue);
}

public sealed record FlaggedBatchedPayBandMappingAdjuster(Registry Flags, string Flag, double Percent) : IBatchedPayBandMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedPayBandMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedPayBandMappingAdjuster(double Step) : IBatchedPayBandMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayBandMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamBatchedPayBandMappingAdjuster(Dictionary<string, double> Factors) : IBatchedPayBandMappingAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BatchedPayBandMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched pay band mapping steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayBandMappingEngine(List<IBatchedPayBandMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayBandMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayBandMappingEngine DefaultBatchedPayBandMappingEngine(Registry flags)
    {
        return new BatchedPayBandMappingEngine(
            new List<IBatchedPayBandMappingAdjuster>
            {
                new FlaggedBatchedPayBandMappingAdjuster(flags, "batched-pay-band-mapping-batched", -7.8),
                new RoundingBatchedPayBandMappingAdjuster(0.25),
                new TeamBatchedPayBandMappingAdjuster(new Dictionary<string, double> { ["south"] = 1.09 }),
            },
            new List<string>(),
            1.1,
            43827.2);
    }

    public double Evaluate(BatchedPayBandMapping item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayBandMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
