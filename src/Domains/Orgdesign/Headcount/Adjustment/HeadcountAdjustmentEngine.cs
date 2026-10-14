namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the headcount adjustment calculation.</summary>
public interface IHeadcountAdjustmentAdjuster
{
    string Describe();
    double Adjust(HeadcountAdjustment item, double valueValue);
}

public sealed record FlaggedHeadcountAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHeadcountAdjustmentAdjuster(double Cap) : IHeadcountAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record HeadcountAdjustmentMinimumOffsetAdjuster(double Weight) : IHeadcountAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(HeadcountAdjustment item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the headcount adjustment steps in order and keeps a trace of each.</summary>
public sealed class HeadcountAdjustmentEngine(List<IHeadcountAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountAdjustmentEngine DefaultHeadcountAdjustmentEngine(Registry flags)
    {
        return new HeadcountAdjustmentEngine(
            new List<IHeadcountAdjustmentAdjuster>
            {
                new FlaggedHeadcountAdjustmentAdjuster(flags, "headcount-adjustment-pending", 2.7),
                new CapHeadcountAdjustmentAdjuster(5976.0),
                new HeadcountAdjustmentMinimumOffsetAdjuster(0.54),
            },
            new List<string>(),
            1.8,
            36624.5);
    }

    public double Run(HeadcountAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
