namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop group calculation.</summary>
public interface ILaptopGroupStep
{
    string Describe();
    double Adjust(LaptopGroup item, double valueValue);
}

public sealed record FlaggedLaptopGroupAdjuster(Registry Flags, string Flag, double Percent) : ILaptopGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopGroupAdjuster(double Cap) : ILaptopGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopGroupAdjuster(double Minimum) : ILaptopGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the laptop group steps in order and keeps a trace of each.</summary>
public sealed class LaptopGroupEngine(List<ILaptopGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopGroupEngine DefaultLaptopGroupEngine(Registry flags)
    {
        return new LaptopGroupEngine(
            new List<ILaptopGroupStep>
            {
                new FlaggedLaptopGroupAdjuster(flags, "laptop-group-tiered", -8.4),
                new CapLaptopGroupAdjuster(16292.0),
                new MinimumLaptopGroupAdjuster(25.4),
            },
            new List<string>(),
            1.9,
            59030.5);
    }

    public double Price(LaptopGroup item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
