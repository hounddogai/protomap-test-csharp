namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched rate baseline calculation.</summary>
public interface IBatchedRateBaselineModifier
{
    string Describe();
    double Adjust(BatchedRateBaseline item, double valueValue);
}

public sealed record FlaggedBatchedRateBaselineAdjuster(Registry Flags, string Flag, double Percent) : IBatchedRateBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedRateBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedRateBaselineAdjuster(double Step) : IBatchedRateBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedRateBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameBatchedRateBaselineAdjuster(Dictionary<string, double> Factors) : IBatchedRateBaselineModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedRateBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched rate baseline steps in order and keeps a trace of each.</summary>
public sealed class BatchedRateBaselineEngine(List<IBatchedRateBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRateBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRateBaselineEngine DefaultBatchedRateBaselineEngine(Registry flags)
    {
        return new BatchedRateBaselineEngine(
            new List<IBatchedRateBaselineModifier>
            {
                new FlaggedBatchedRateBaselineAdjuster(flags, "batched-rate-baseline-tiered", -8.8),
                new RoundingBatchedRateBaselineAdjuster(0.5),
                new NameBatchedRateBaselineAdjuster(new Dictionary<string, double> { ["online"] = 0.82 }),
            },
            new List<string>(),
            4.5,
            26141.3);
    }

    public double Compute(BatchedRateBaseline item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRateBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
