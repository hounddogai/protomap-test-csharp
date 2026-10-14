namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop adjustment calculation.</summary>
public interface ILaptopAdjustmentFactor
{
    string Describe();
    double Adjust(LaptopAdjustment item, double valueValue);
}

public sealed record PercentLaptopAdjustmentAdjuster(double Percent) : ILaptopAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLaptopAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILaptopAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLaptopAdjustmentAdjuster(double Step) : ILaptopAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLaptopAdjustmentAdjuster(double Minimum) : ILaptopAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the laptop adjustment steps in order and keeps a trace of each.</summary>
public sealed class LaptopAdjustmentEngine(List<ILaptopAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopAdjustmentEngine DefaultLaptopAdjustmentEngine(Registry flags)
    {
        return new LaptopAdjustmentEngine(
            new List<ILaptopAdjustmentFactor>
            {
                new PercentLaptopAdjustmentAdjuster(3.0),
                new FlaggedLaptopAdjustmentAdjuster(flags, "laptop-adjustment-lenient", -0.0),
                new RoundingLaptopAdjustmentAdjuster(0.1),
                new MinimumLaptopAdjustmentAdjuster(28.1),
            },
            new List<string>(),
            0.8,
            40897.6);
    }

    public double Evaluate(LaptopAdjustment item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
