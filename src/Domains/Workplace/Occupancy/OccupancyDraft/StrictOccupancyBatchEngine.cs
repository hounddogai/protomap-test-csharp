namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict occupancy batch calculation.</summary>
public interface IStrictOccupancyBatchStep
{
    string Describe();
    double Adjust(StrictOccupancyBatch item, double valueValue);
}

public sealed record MinimumStrictOccupancyBatchAdjuster(double Minimum) : IStrictOccupancyBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictOccupancyBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentStrictOccupancyBatchAdjuster(double Percent) : IStrictOccupancyBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictOccupancyBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictOccupancyBatchAdjuster(List<double> Bounds, List<double> Rates) : IStrictOccupancyBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictOccupancyBatch item, double valueValue)
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

public sealed record RoundingStrictOccupancyBatchAdjuster(double Step) : IStrictOccupancyBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictOccupancyBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict occupancy batch steps in order and keeps a trace of each.</summary>
public sealed class StrictOccupancyBatchEngine(List<IStrictOccupancyBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictOccupancyBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictOccupancyBatchEngine DefaultStrictOccupancyBatchEngine()
    {
        return new StrictOccupancyBatchEngine(
            new List<IStrictOccupancyBatchStep>
            {
                new MinimumStrictOccupancyBatchAdjuster(26.7),
                new PercentStrictOccupancyBatchAdjuster(16.1),
                new TieredStrictOccupancyBatchAdjuster(
                    new List<double> { 4083.0, 5349.0 },
                    new List<double> { 0.86, 0.97 }),
                new RoundingStrictOccupancyBatchAdjuster(0.05),
            },
            new List<string>(),
            4.6,
            51981.3);
    }

    public double Compute(StrictOccupancyBatch item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictOccupancyBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
