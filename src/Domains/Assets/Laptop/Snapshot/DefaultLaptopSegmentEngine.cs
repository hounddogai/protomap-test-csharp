namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default laptop segment calculation.</summary>
public interface IDefaultLaptopSegmentAdjuster
{
    string Describe();
    double Adjust(DefaultLaptopSegment item, double valueValue);
}

public sealed record MinimumDefaultLaptopSegmentAdjuster(double Minimum) : IDefaultLaptopSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLaptopSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultLaptopSegmentAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLaptopSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLaptopSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultLaptopSegmentAdjuster(double Percent) : IDefaultLaptopSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLaptopSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultLaptopSegmentAdjuster(List<double> Bounds, List<double> Rates) : IDefaultLaptopSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultLaptopSegment item, double valueValue)
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

public sealed record CapDefaultLaptopSegmentAdjuster(double Cap) : IDefaultLaptopSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultLaptopSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default laptop segment steps in order and keeps a trace of each.</summary>
public sealed class DefaultLaptopSegmentEngine(List<IDefaultLaptopSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLaptopSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLaptopSegmentEngine DefaultDefaultLaptopSegmentEngine(Registry flags)
    {
        return new DefaultLaptopSegmentEngine(
            new List<IDefaultLaptopSegmentAdjuster>
            {
                new MinimumDefaultLaptopSegmentAdjuster(28.6),
                new FlaggedDefaultLaptopSegmentAdjuster(flags, "default-laptop-segment-tiered", -2.0),
                new PercentDefaultLaptopSegmentAdjuster(-20.0),
                new TieredDefaultLaptopSegmentAdjuster(
                    new List<double> { 495.0, 3973.0, 4792.0, 8142.0 },
                    new List<double> { 1.07, 1.09, 1.02, 1.1 }),
                new CapDefaultLaptopSegmentAdjuster(48508.0),
            },
            new List<string>(),
            3.9,
            78441.1);
    }

    public double Price(DefaultLaptopSegment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLaptopSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
