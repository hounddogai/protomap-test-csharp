namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment slot calculation.</summary>
public interface IAdjustmentSlotFactor
{
    string Describe();
    double Adjust(AdjustmentSlot item, double valueValue);
}

public sealed record PercentAdjustmentSlotAdjuster(double Percent) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentSlotAdjuster(double Minimum) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdjustmentSlotAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleAdjustmentSlotAdjuster(Dictionary<string, double> Factors) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdjustmentSlotAdjuster(double Step) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AdjustmentSlotGraceDaysOffsetAdjuster(double Weight) : IAdjustmentSlotFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(AdjustmentSlot item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the adjustment slot steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentSlotEngine(List<IAdjustmentSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentSlotEngine DefaultAdjustmentSlotEngine(Registry flags)
    {
        return new AdjustmentSlotEngine(
            new List<IAdjustmentSlotFactor>
            {
                new PercentAdjustmentSlotAdjuster(21.2),
                new MinimumAdjustmentSlotAdjuster(44.7),
                new FlaggedAdjustmentSlotAdjuster(flags, "adjustment-slot-global", -5.4),
                new TitleAdjustmentSlotAdjuster(new Dictionary<string, double> { ["central"] = 1.23 }),
                new RoundingAdjustmentSlotAdjuster(5.0),
                new AdjustmentSlotGraceDaysOffsetAdjuster(1.9),
            },
            new List<string>(),
            0.9,
            12561.0);
    }

    public double Run(AdjustmentSlot item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
