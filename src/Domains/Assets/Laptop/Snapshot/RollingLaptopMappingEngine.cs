namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling laptop mapping calculation.</summary>
public interface IRollingLaptopMappingModifier
{
    string Describe();
    double Adjust(RollingLaptopMapping item, double valueValue);
}

public sealed record TieredRollingLaptopMappingAdjuster(List<double> Bounds, List<double> Rates) : IRollingLaptopMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingLaptopMapping item, double valueValue)
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

public sealed record FlaggedRollingLaptopMappingAdjuster(Registry Flags, string Flag, double Percent) : IRollingLaptopMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLaptopMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingLaptopMappingAdjuster(double Cap) : IRollingLaptopMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingLaptopMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingLaptopMappingAmountOffsetAdjuster(double Weight) : IRollingLaptopMappingModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RollingLaptopMapping item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumRollingLaptopMappingAdjuster(double Minimum) : IRollingLaptopMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingLaptopMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling laptop mapping steps in order and keeps a trace of each.</summary>
public sealed class RollingLaptopMappingEngine(List<IRollingLaptopMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLaptopMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLaptopMappingEngine DefaultRollingLaptopMappingEngine(Registry flags)
    {
        return new RollingLaptopMappingEngine(
            new List<IRollingLaptopMappingModifier>
            {
                new TieredRollingLaptopMappingAdjuster(
                    new List<double> { 1351.0, 3813.0, 9419.0 },
                    new List<double> { 0.99, 1.19, 1.15 }),
                new FlaggedRollingLaptopMappingAdjuster(flags, "rolling-laptop-mapping-lenient", -0.8),
                new CapRollingLaptopMappingAdjuster(43545.0),
                new RollingLaptopMappingAmountOffsetAdjuster(0.46),
                new MinimumRollingLaptopMappingAdjuster(47.9),
            },
            new List<string>(),
            1.3,
            33574.9);
    }

    public double Run(RollingLaptopMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingLaptopMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
