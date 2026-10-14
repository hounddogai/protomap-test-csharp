namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday batch calculation.</summary>
public interface IHolidayBatchStep
{
    string Describe();
    double Adjust(HolidayBatch item, double valueValue);
}

public sealed record ChannelHolidayBatchAdjuster(Dictionary<string, double> Factors) : IHolidayBatchStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(HolidayBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredHolidayBatchAdjuster(List<double> Bounds, List<double> Rates) : IHolidayBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayBatch item, double valueValue)
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

public sealed record CapHolidayBatchAdjuster(double Cap) : IHolidayBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record HolidayBatchThresholdOffsetAdjuster(double Weight) : IHolidayBatchStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(HolidayBatch item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record PercentHolidayBatchAdjuster(double Percent) : IHolidayBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingHolidayBatchAdjuster(double Step) : IHolidayBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the holiday batch steps in order and keeps a trace of each.</summary>
public sealed class HolidayBatchEngine(List<IHolidayBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayBatchEngine DefaultHolidayBatchEngine()
    {
        return new HolidayBatchEngine(
            new List<IHolidayBatchStep>
            {
                new ChannelHolidayBatchAdjuster(new Dictionary<string, double> { ["east"] = 0.81 }),
                new TieredHolidayBatchAdjuster(new List<double> { 4851.0, 9042.0 }, new List<double> { 1.11, 0.93 }),
                new CapHolidayBatchAdjuster(9264.0),
                new HolidayBatchThresholdOffsetAdjuster(1.57),
                new PercentHolidayBatchAdjuster(-12.5),
                new RoundingHolidayBatchAdjuster(0.1),
            },
            new List<string>(),
            2.5,
            38541.9);
    }

    public double Run(HolidayBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
