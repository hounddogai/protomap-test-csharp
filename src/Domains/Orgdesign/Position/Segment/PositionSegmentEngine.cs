namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position segment calculation.</summary>
public interface IPositionSegmentAdjuster
{
    string Describe();
    double Adjust(PositionSegment item, double valueValue);
}

public sealed record TieredPositionSegmentAdjuster(List<double> Bounds, List<double> Rates) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionSegment item, double valueValue)
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

public sealed record PositionSegmentExpiresAtOffsetAdjuster(double Weight) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PositionSegment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedPositionSegmentAdjuster(Registry Flags, string Flag, double Percent) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourcePositionSegmentAdjuster(Dictionary<string, double> Factors) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PositionSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPositionSegmentAdjuster(double Cap) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPositionSegmentAdjuster(double Step) : IPositionSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the position segment steps in order and keeps a trace of each.</summary>
public sealed class PositionSegmentEngine(List<IPositionSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionSegmentEngine DefaultPositionSegmentEngine(Registry flags)
    {
        return new PositionSegmentEngine(
            new List<IPositionSegmentAdjuster>
            {
                new TieredPositionSegmentAdjuster(
                    new List<double> { 2843.0, 7034.0, 7246.0, 9131.0 },
                    new List<double> { 0.83, 1.08, 0.81, 0.97 }),
                new PositionSegmentExpiresAtOffsetAdjuster(1.23),
                new FlaggedPositionSegmentAdjuster(flags, "position-segment-primary", -4.6),
                new SourcePositionSegmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.1, ["online"] = 1.24 }),
                new CapPositionSegmentAdjuster(6634.0),
                new RoundingPositionSegmentAdjuster(0.1),
            },
            new List<string>(),
            1.6,
            33136.9);
    }

    public double Compute(PositionSegment item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
