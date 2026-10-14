namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop threshold calculation.</summary>
public interface ILaptopThresholdAdjuster
{
    string Describe();
    double Adjust(LaptopThreshold item, double valueValue);
}

public sealed record LaptopThresholdDiscountOffsetAdjuster(double Weight) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record FlaggedLaptopThresholdAdjuster(Registry Flags, string Flag, double Percent) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLaptopThresholdAdjuster(double Percent) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLaptopThresholdAdjuster(double Step) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLaptopThresholdAdjuster(double Cap) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelLaptopThresholdAdjuster(Dictionary<string, double> Factors) : ILaptopThresholdAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LaptopThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the laptop threshold steps in order and keeps a trace of each.</summary>
public sealed class LaptopThresholdEngine(List<ILaptopThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopThresholdEngine DefaultLaptopThresholdEngine(Registry flags)
    {
        return new LaptopThresholdEngine(
            new List<ILaptopThresholdAdjuster>
            {
                new LaptopThresholdDiscountOffsetAdjuster(0.79),
                new FlaggedLaptopThresholdAdjuster(flags, "laptop-threshold-fallback", 3.8),
                new PercentLaptopThresholdAdjuster(9.2),
                new RoundingLaptopThresholdAdjuster(0.5),
                new CapLaptopThresholdAdjuster(10868.0),
                new ChannelLaptopThresholdAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.14, ["west"] = 1.08, ["south"] = 1.0 }),
            },
            new List<string>(),
            4.9,
            10804.6);
    }

    public double Run(LaptopThreshold item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
