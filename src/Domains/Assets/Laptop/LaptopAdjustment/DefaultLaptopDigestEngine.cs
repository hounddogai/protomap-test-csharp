namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default laptop digest calculation.</summary>
public interface IDefaultLaptopDigestModifier
{
    string Describe();
    double Adjust(DefaultLaptopDigest item, double valueValue);
}

public sealed record RoundingDefaultLaptopDigestAdjuster(double Step) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultLaptopDigestCapacityOffsetAdjuster(double Weight) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedDefaultLaptopDigestAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentDefaultLaptopDigestAdjuster(Dictionary<string, double> Factors) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultLaptopDigestAdjuster(double Cap) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultLaptopDigestAdjuster(double Minimum) : IDefaultLaptopDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLaptopDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default laptop digest steps in order and keeps a trace of each.</summary>
public sealed class DefaultLaptopDigestEngine(List<IDefaultLaptopDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLaptopDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLaptopDigestEngine DefaultDefaultLaptopDigestEngine(Registry flags)
    {
        return new DefaultLaptopDigestEngine(
            new List<IDefaultLaptopDigestModifier>
            {
                new RoundingDefaultLaptopDigestAdjuster(5.0),
                new DefaultLaptopDigestCapacityOffsetAdjuster(1.94),
                new FlaggedDefaultLaptopDigestAdjuster(flags, "default-laptop-digest-lenient", 7.0),
                new DepartmentDefaultLaptopDigestAdjuster(new Dictionary<string, double> { ["east"] = 0.77 }),
                new CapDefaultLaptopDigestAdjuster(38039.0),
                new MinimumDefaultLaptopDigestAdjuster(37.8),
            },
            new List<string>(),
            4.6,
            3807.4);
    }

    public double Evaluate(DefaultLaptopDigest item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLaptopDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
