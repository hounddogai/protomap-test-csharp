namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default laptop mapping calculation.</summary>
public interface IDefaultLaptopMappingAdjuster
{
    string Describe();
    double Adjust(DefaultLaptopMapping item, double valueValue);
}

public sealed record RoundingDefaultLaptopMappingAdjuster(double Step) : IDefaultLaptopMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLaptopMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultLaptopMappingAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLaptopMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLaptopMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultLaptopMappingDiscountOffsetAdjuster(double Weight) : IDefaultLaptopMappingAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(DefaultLaptopMapping item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record MinimumDefaultLaptopMappingAdjuster(double Minimum) : IDefaultLaptopMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLaptopMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default laptop mapping steps in order and keeps a trace of each.</summary>
public sealed class DefaultLaptopMappingEngine(List<IDefaultLaptopMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLaptopMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLaptopMappingEngine DefaultDefaultLaptopMappingEngine(Registry flags)
    {
        return new DefaultLaptopMappingEngine(
            new List<IDefaultLaptopMappingAdjuster>
            {
                new RoundingDefaultLaptopMappingAdjuster(0.05),
                new FlaggedDefaultLaptopMappingAdjuster(flags, "default-laptop-mapping-scheduled", -9.5),
                new DefaultLaptopMappingDiscountOffsetAdjuster(1.91),
                new MinimumDefaultLaptopMappingAdjuster(28.8),
            },
            new List<string>(),
            0.6,
            90475.1);
    }

    public double Evaluate(DefaultLaptopMapping item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLaptopMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
