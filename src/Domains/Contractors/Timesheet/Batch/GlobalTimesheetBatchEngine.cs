namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global timesheet batch calculation.</summary>
public interface IGlobalTimesheetBatchAdjuster
{
    string Describe();
    double Adjust(GlobalTimesheetBatch item, double valueValue);
}

public sealed record PercentGlobalTimesheetBatchAdjuster(double Percent) : IGlobalTimesheetBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalTimesheetBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalTimesheetBatchAdjuster(double Step) : IGlobalTimesheetBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalTimesheetBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGlobalTimesheetBatchAdjuster(double Minimum) : IGlobalTimesheetBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalTimesheetBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalTimesheetBatchAdjuster(List<double> Bounds, List<double> Rates) : IGlobalTimesheetBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalTimesheetBatch item, double valueValue)
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

public sealed record GlobalTimesheetBatchRateOffsetAdjuster(double Weight) : IGlobalTimesheetBatchAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(GlobalTimesheetBatch item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the global timesheet batch steps in order and keeps a trace of each.</summary>
public sealed class GlobalTimesheetBatchEngine(List<IGlobalTimesheetBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalTimesheetBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalTimesheetBatchEngine DefaultGlobalTimesheetBatchEngine()
    {
        return new GlobalTimesheetBatchEngine(
            new List<IGlobalTimesheetBatchAdjuster>
            {
                new PercentGlobalTimesheetBatchAdjuster(-12.6),
                new RoundingGlobalTimesheetBatchAdjuster(0.5),
                new MinimumGlobalTimesheetBatchAdjuster(45.4),
                new TieredGlobalTimesheetBatchAdjuster(
                    new List<double> { 5249.0, 9830.0 },
                    new List<double> { 1.0, 0.99 }),
                new GlobalTimesheetBatchRateOffsetAdjuster(0.54),
            },
            new List<string>(),
            1.3,
            1963.7);
    }

    public double Run(GlobalTimesheetBatch item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalTimesheetBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
