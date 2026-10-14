namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the holiday metric calculation.</summary>
public interface IHolidayMetricFactor
{
    string Describe();
    double Adjust(HolidayMetric item, double valueValue);
}

public sealed record FlaggedHolidayMetricAdjuster(Registry Flags, string Flag, double Percent) : IHolidayMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HolidayMetricGraceDaysOffsetAdjuster(double Weight) : IHolidayMetricFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(HolidayMetric item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record DepartmentHolidayMetricAdjuster(Dictionary<string, double> Factors) : IHolidayMetricFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(HolidayMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredHolidayMetricAdjuster(List<double> Bounds, List<double> Rates) : IHolidayMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayMetric item, double valueValue)
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

public sealed record CapHolidayMetricAdjuster(double Cap) : IHolidayMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the holiday metric steps in order and keeps a trace of each.</summary>
public sealed class HolidayMetricEngine(List<IHolidayMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayMetricEngine DefaultHolidayMetricEngine(Registry flags)
    {
        return new HolidayMetricEngine(
            new List<IHolidayMetricFactor>
            {
                new FlaggedHolidayMetricAdjuster(flags, "holiday-metric-batched", 2.9),
                new HolidayMetricGraceDaysOffsetAdjuster(0.9),
                new DepartmentHolidayMetricAdjuster(new Dictionary<string, double> { ["retail"] = 1.06 }),
                new TieredHolidayMetricAdjuster(new List<double> { 1650.0, 6896.0 }, new List<double> { 0.89, 1.04 }),
                new CapHolidayMetricAdjuster(12823.0),
            },
            new List<string>(),
            3.1,
            10953.1);
    }

    public double Compute(HolidayMetric item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
