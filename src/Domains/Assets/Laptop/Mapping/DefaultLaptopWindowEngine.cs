namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default laptop window calculation.</summary>
public interface IDefaultLaptopWindowStep
{
    string Describe();
    double Adjust(DefaultLaptopWindow item, double valueValue);
}

public sealed record MinimumDefaultLaptopWindowAdjuster(double Minimum) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultLaptopWindowAdjuster(double Percent) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultLaptopWindowAdjuster(List<double> Bounds, List<double> Rates) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
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

public sealed record FlaggedDefaultLaptopWindowAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultLaptopWindowAdjuster(double Cap) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultLaptopWindowRateOffsetAdjuster(double Weight) : IDefaultLaptopWindowStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(DefaultLaptopWindow item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the default laptop window steps in order and keeps a trace of each.</summary>
public sealed class DefaultLaptopWindowEngine(List<IDefaultLaptopWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLaptopWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLaptopWindowEngine DefaultDefaultLaptopWindowEngine(Registry flags)
    {
        return new DefaultLaptopWindowEngine(
            new List<IDefaultLaptopWindowStep>
            {
                new MinimumDefaultLaptopWindowAdjuster(21.4),
                new PercentDefaultLaptopWindowAdjuster(18.4),
                new TieredDefaultLaptopWindowAdjuster(
                    new List<double> { 1668.0, 2801.0, 5525.0 },
                    new List<double> { 1.05, 0.88, 0.96 }),
                new FlaggedDefaultLaptopWindowAdjuster(flags, "default-laptop-window-seasonal", -2.1),
                new CapDefaultLaptopWindowAdjuster(43194.0),
                new DefaultLaptopWindowRateOffsetAdjuster(1.55),
            },
            new List<string>(),
            4.9,
            63201.2);
    }

    public double Compute(DefaultLaptopWindow item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLaptopWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
