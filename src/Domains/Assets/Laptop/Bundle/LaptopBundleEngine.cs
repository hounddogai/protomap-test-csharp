namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop bundle calculation.</summary>
public interface ILaptopBundleAdjuster
{
    string Describe();
    double Adjust(LaptopBundle item, double valueValue);
}

public sealed record RoundingLaptopBundleAdjuster(double Step) : ILaptopBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLaptopBundleAdjuster(Registry Flags, string Flag, double Percent) : ILaptopBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLaptopBundleAdjuster(double Percent) : ILaptopBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the laptop bundle steps in order and keeps a trace of each.</summary>
public sealed class LaptopBundleEngine(List<ILaptopBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopBundleEngine DefaultLaptopBundleEngine(Registry flags)
    {
        return new LaptopBundleEngine(
            new List<ILaptopBundleAdjuster>
            {
                new RoundingLaptopBundleAdjuster(5.0),
                new FlaggedLaptopBundleAdjuster(flags, "laptop-bundle-primary", 9.4),
                new PercentLaptopBundleAdjuster(-11.6),
            },
            new List<string>(),
            4.4,
            21887.5);
    }

    public double Compute(LaptopBundle item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
