namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount batch calculation.</summary>
public interface IHeadcountBatchStep
{
    string Describe();
    double Adjust(HeadcountBatch item, double valueValue);
}

public sealed record PercentHeadcountBatchAdjuster(double Percent) : IHeadcountBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedHeadcountBatchAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHeadcountBatchAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountBatch item, double valueValue)
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

public sealed record RoundingHeadcountBatchAdjuster(double Step) : IHeadcountBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the headcount batch steps in order and keeps a trace of each.</summary>
public sealed class HeadcountBatchEngine(List<IHeadcountBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountBatchEngine DefaultHeadcountBatchEngine(Registry flags)
    {
        return new HeadcountBatchEngine(
            new List<IHeadcountBatchStep>
            {
                new PercentHeadcountBatchAdjuster(18.4),
                new FlaggedHeadcountBatchAdjuster(flags, "headcount-batch-adaptive", 4.8),
                new TieredHeadcountBatchAdjuster(
                    new List<double> { 261.0, 2415.0, 3753.0 },
                    new List<double> { 1.18, 1.11, 0.89 }),
                new RoundingHeadcountBatchAdjuster(0.1),
            },
            new List<string>(),
            2.2,
            1805.9);
    }

    public double Compute(HeadcountBatch item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
