namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered vendor schedule calculation.</summary>
public interface ITieredVendorScheduleAdjuster
{
    string Describe();
    double Adjust(TieredVendorSchedule item, double valueValue);
}

public sealed record MinimumTieredVendorScheduleAdjuster(double Minimum) : ITieredVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredVendorSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTieredVendorScheduleAdjuster(Registry Flags, string Flag, double Percent) : ITieredVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredVendorSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredVendorScheduleAdjuster(double Cap) : ITieredVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredVendorSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelTieredVendorScheduleAdjuster(Dictionary<string, double> Factors) : ITieredVendorScheduleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredVendorSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredVendorScheduleEndsAtOffsetAdjuster(double Weight) : ITieredVendorScheduleAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(TieredVendorSchedule item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the tiered vendor schedule steps in order and keeps a trace of each.</summary>
public sealed class TieredVendorScheduleEngine(List<ITieredVendorScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredVendorScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredVendorScheduleEngine DefaultTieredVendorScheduleEngine(Registry flags)
    {
        return new TieredVendorScheduleEngine(
            new List<ITieredVendorScheduleAdjuster>
            {
                new MinimumTieredVendorScheduleAdjuster(46.1),
                new FlaggedTieredVendorScheduleAdjuster(flags, "tiered-vendor-schedule-capped", -0.8),
                new CapTieredVendorScheduleAdjuster(12887.0),
                new LabelTieredVendorScheduleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["east"] = 1.2, ["south"] = 1.27 }),
                new TieredVendorScheduleEndsAtOffsetAdjuster(0.77),
            },
            new List<string>(),
            0.1,
            31089.9);
    }

    public double Compute(TieredVendorSchedule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredVendorSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
