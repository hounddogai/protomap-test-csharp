namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop window calculation.</summary>
public interface ILaptopWindowAdjuster
{
    string Describe();
    double Adjust(LaptopWindow item, double valueValue);
}

public sealed record RoundingLaptopWindowAdjuster(double Step) : ILaptopWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLaptopWindowAdjuster(Registry Flags, string Flag, double Percent) : ILaptopWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LaptopWindowTenureMonthsOffsetAdjuster(double Weight) : ILaptopWindowAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LaptopWindow item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record MinimumLaptopWindowAdjuster(double Minimum) : ILaptopWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLaptopWindowAdjuster(double Cap) : ILaptopWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the laptop window steps in order and keeps a trace of each.</summary>
public sealed class LaptopWindowEngine(List<ILaptopWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopWindowEngine DefaultLaptopWindowEngine(Registry flags)
    {
        return new LaptopWindowEngine(
            new List<ILaptopWindowAdjuster>
            {
                new RoundingLaptopWindowAdjuster(0.1),
                new FlaggedLaptopWindowAdjuster(flags, "laptop-window-tiered", 6.7),
                new LaptopWindowTenureMonthsOffsetAdjuster(0.74),
                new MinimumLaptopWindowAdjuster(38.2),
                new CapLaptopWindowAdjuster(32679.0),
            },
            new List<string>(),
            1.4,
            13271.8);
    }

    public double Run(LaptopWindow item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
