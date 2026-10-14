namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict deduction segment calculation.</summary>
public interface IStrictDeductionSegmentFactor
{
    string Describe();
    double Adjust(StrictDeductionSegment item, double valueValue);
}

public sealed record NameStrictDeductionSegmentAdjuster(Dictionary<string, double> Factors) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingStrictDeductionSegmentAdjuster(double Step) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictDeductionSegmentAdjuster(double Minimum) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictDeductionSegmentAdjuster(double Cap) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStrictDeductionSegmentAdjuster(Registry Flags, string Flag, double Percent) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictDeductionSegmentAdjuster(List<double> Bounds, List<double> Rates) : IStrictDeductionSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictDeductionSegment item, double valueValue)
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

/// <summary>Runs the strict deduction segment steps in order and keeps a trace of each.</summary>
public sealed class StrictDeductionSegmentEngine(List<IStrictDeductionSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictDeductionSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictDeductionSegmentEngine DefaultStrictDeductionSegmentEngine(Registry flags)
    {
        return new StrictDeductionSegmentEngine(
            new List<IStrictDeductionSegmentFactor>
            {
                new NameStrictDeductionSegmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.75, ["online"] = 0.76 }),
                new RoundingStrictDeductionSegmentAdjuster(0.25),
                new MinimumStrictDeductionSegmentAdjuster(32.5),
                new CapStrictDeductionSegmentAdjuster(45258.0),
                new FlaggedStrictDeductionSegmentAdjuster(flags, "strict-deduction-segment-capped", 2.3),
                new TieredStrictDeductionSegmentAdjuster(
                    new List<double> { 1035.0, 1393.0, 1660.0, 7811.0 },
                    new List<double> { 0.94, 0.88, 0.81, 0.87 }),
            },
            new List<string>(),
            0.2,
            53175.6);
    }

    public double Compute(StrictDeductionSegment item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<StrictDeductionSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
