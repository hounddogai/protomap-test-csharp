namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk segment calculation.</summary>
public interface IDeskSegmentModifier
{
    string Describe();
    double Adjust(DeskSegment item, double valueValue);
}

public sealed record UnitDeskSegmentAdjuster(Dictionary<string, double> Factors) : IDeskSegmentModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DeskSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDeskSegmentAdjuster(double Step) : IDeskSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDeskSegmentAdjuster(Registry Flags, string Flag, double Percent) : IDeskSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk segment steps in order and keeps a trace of each.</summary>
public sealed class DeskSegmentEngine(List<IDeskSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskSegmentEngine DefaultDeskSegmentEngine(Registry flags)
    {
        return new DeskSegmentEngine(
            new List<IDeskSegmentModifier>
            {
                new UnitDeskSegmentAdjuster(new Dictionary<string, double> { ["west"] = 1.31 }),
                new RoundingDeskSegmentAdjuster(0.5),
                new FlaggedDeskSegmentAdjuster(flags, "desk-segment-adaptive", -2.8),
            },
            new List<string>(),
            2.6,
            75119.8);
    }

    public double Price(DeskSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
