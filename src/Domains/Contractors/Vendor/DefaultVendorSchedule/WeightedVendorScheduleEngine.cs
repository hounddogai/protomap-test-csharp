namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted vendor schedule calculation.</summary>
public interface IWeightedVendorScheduleAdjuster
{
    string Describe();
    double Adjust(WeightedVendorSchedule item, double valueValue);
}

public sealed record WeightedVendorScheduleBatchSizeOffsetAdjuster(double Weight) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record FlaggedWeightedVendorScheduleAdjuster(Registry Flags, string Flag, double Percent) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedVendorScheduleAdjuster(List<double> Bounds, List<double> Rates) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
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

public sealed record MinimumWeightedVendorScheduleAdjuster(double Minimum) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedVendorScheduleAdjuster(double Step) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWeightedVendorScheduleAdjuster(double Percent) : IWeightedVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedVendorSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted vendor schedule steps in order and keeps a trace of each.</summary>
public sealed class WeightedVendorScheduleEngine(List<IWeightedVendorScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedVendorScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedVendorScheduleEngine DefaultWeightedVendorScheduleEngine(Registry flags)
    {
        return new WeightedVendorScheduleEngine(
            new List<IWeightedVendorScheduleAdjuster>
            {
                new WeightedVendorScheduleBatchSizeOffsetAdjuster(1.07),
                new FlaggedWeightedVendorScheduleAdjuster(flags, "weighted-vendor-schedule-strict", -0.6),
                new TieredWeightedVendorScheduleAdjuster(
                    new List<double> { 2072.0, 2588.0 },
                    new List<double> { 0.97, 1.07 }),
                new MinimumWeightedVendorScheduleAdjuster(16.8),
                new RoundingWeightedVendorScheduleAdjuster(1.0),
                new PercentWeightedVendorScheduleAdjuster(-4.9),
            },
            new List<string>(),
            1.5,
            14858.0);
    }

    public double Run(WeightedVendorSchedule item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedVendorSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
