namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop target calculation.</summary>
public interface ILaptopTargetAdjuster
{
    string Describe();
    double Adjust(LaptopTarget item, double valueValue);
}

public sealed record FlaggedLaptopTargetAdjuster(Registry Flags, string Flag, double Percent) : ILaptopTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLaptopTargetAdjuster(double Step) : ILaptopTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LaptopTargetRateOffsetAdjuster(double Weight) : ILaptopTargetAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(LaptopTarget item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredLaptopTargetAdjuster(List<double> Bounds, List<double> Rates) : ILaptopTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopTarget item, double valueValue)
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

public sealed record MinimumLaptopTargetAdjuster(double Minimum) : ILaptopTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the laptop target steps in order and keeps a trace of each.</summary>
public sealed class LaptopTargetEngine(List<ILaptopTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopTargetEngine DefaultLaptopTargetEngine(Registry flags)
    {
        return new LaptopTargetEngine(
            new List<ILaptopTargetAdjuster>
            {
                new FlaggedLaptopTargetAdjuster(flags, "laptop-target-regional", 9.5),
                new RoundingLaptopTargetAdjuster(0.1),
                new LaptopTargetRateOffsetAdjuster(0.54),
                new TieredLaptopTargetAdjuster(new List<double> { 7791.0, 9903.0 }, new List<double> { 0.92, 1.12 }),
                new MinimumLaptopTargetAdjuster(31.5),
            },
            new List<string>(),
            4.5,
            79328.4);
    }

    public double Compute(LaptopTarget item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
