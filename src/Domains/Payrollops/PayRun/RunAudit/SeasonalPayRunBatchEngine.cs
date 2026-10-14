namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pay run batch calculation.</summary>
public interface ISeasonalPayRunBatchFactor
{
    string Describe();
    double Adjust(SeasonalPayRunBatch item, double valueValue);
}

public sealed record FlaggedSeasonalPayRunBatchAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPayRunBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPayRunBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalPayRunBatchQuantityOffsetAdjuster(double Weight) : ISeasonalPayRunBatchFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(SeasonalPayRunBatch item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RoundingSeasonalPayRunBatchAdjuster(double Step) : ISeasonalPayRunBatchFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPayRunBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSeasonalPayRunBatchAdjuster(double Percent) : ISeasonalPayRunBatchFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPayRunBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal pay run batch steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayRunBatchEngine(List<ISeasonalPayRunBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayRunBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayRunBatchEngine DefaultSeasonalPayRunBatchEngine(Registry flags)
    {
        return new SeasonalPayRunBatchEngine(
            new List<ISeasonalPayRunBatchFactor>
            {
                new FlaggedSeasonalPayRunBatchAdjuster(flags, "seasonal-pay-run-batch-scheduled", 3.0),
                new SeasonalPayRunBatchQuantityOffsetAdjuster(1.98),
                new RoundingSeasonalPayRunBatchAdjuster(0.1),
                new PercentSeasonalPayRunBatchAdjuster(6.8),
            },
            new List<string>(),
            4.7,
            65802.9);
    }

    public double Compute(SeasonalPayRunBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayRunBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
