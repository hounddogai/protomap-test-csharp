namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped laptop segment calculation.</summary>
public interface ICappedLaptopSegmentStep
{
    string Describe();
    double Adjust(CappedLaptopSegment item, double valueValue);
}

public sealed record FlaggedCappedLaptopSegmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedLaptopSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedLaptopSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedLaptopSegmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedLaptopSegmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedLaptopSegment item, double valueValue)
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

public sealed record RoundingCappedLaptopSegmentAdjuster(double Step) : ICappedLaptopSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedLaptopSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedLaptopSegmentRatioOffsetAdjuster(double Weight) : ICappedLaptopSegmentStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(CappedLaptopSegment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapCappedLaptopSegmentAdjuster(double Cap) : ICappedLaptopSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedLaptopSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped laptop segment steps in order and keeps a trace of each.</summary>
public sealed class CappedLaptopSegmentEngine(List<ICappedLaptopSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedLaptopSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedLaptopSegmentEngine DefaultCappedLaptopSegmentEngine(Registry flags)
    {
        return new CappedLaptopSegmentEngine(
            new List<ICappedLaptopSegmentStep>
            {
                new FlaggedCappedLaptopSegmentAdjuster(flags, "capped-laptop-segment-legacy", 8.4),
                new TieredCappedLaptopSegmentAdjuster(
                    new List<double> { 524.0, 1266.0, 8679.0 },
                    new List<double> { 0.81, 1.13, 1.01 }),
                new RoundingCappedLaptopSegmentAdjuster(1.0),
                new CappedLaptopSegmentRatioOffsetAdjuster(0.59),
                new CapCappedLaptopSegmentAdjuster(34232.0),
            },
            new List<string>(),
            3.6,
            73154.8);
    }

    public double Price(CappedLaptopSegment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedLaptopSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
