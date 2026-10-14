namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted adjustment notice calculation.</summary>
public interface IWeightedAdjustmentNoticeStep
{
    string Describe();
    double Adjust(WeightedAdjustmentNotice item, double valueValue);
}

public sealed record WeightedAdjustmentNoticeTenureMonthsOffsetAdjuster(double Weight) : IWeightedAdjustmentNoticeStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(WeightedAdjustmentNotice item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record CapWeightedAdjustmentNoticeAdjuster(double Cap) : IWeightedAdjustmentNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAdjustmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWeightedAdjustmentNoticeAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAdjustmentNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAdjustmentNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedAdjustmentNoticeAdjuster(double Minimum) : IWeightedAdjustmentNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAdjustmentNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredWeightedAdjustmentNoticeAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAdjustmentNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAdjustmentNotice item, double valueValue)
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

/// <summary>Runs the weighted adjustment notice steps in order and keeps a trace of each.</summary>
public sealed class WeightedAdjustmentNoticeEngine(List<IWeightedAdjustmentNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAdjustmentNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAdjustmentNoticeEngine DefaultWeightedAdjustmentNoticeEngine(Registry flags)
    {
        return new WeightedAdjustmentNoticeEngine(
            new List<IWeightedAdjustmentNoticeStep>
            {
                new WeightedAdjustmentNoticeTenureMonthsOffsetAdjuster(1.41),
                new CapWeightedAdjustmentNoticeAdjuster(6935.0),
                new FlaggedWeightedAdjustmentNoticeAdjuster(flags, "weighted-adjustment-notice-pending", -9.0),
                new MinimumWeightedAdjustmentNoticeAdjuster(4.1),
                new TieredWeightedAdjustmentNoticeAdjuster(
                    new List<double> { 1928.0, 5929.0, 5997.0, 7385.0 },
                    new List<double> { 0.91, 1.04, 1.09, 1.19 }),
            },
            new List<string>(),
            0.5,
            77907.8);
    }

    public double Run(WeightedAdjustmentNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAdjustmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
