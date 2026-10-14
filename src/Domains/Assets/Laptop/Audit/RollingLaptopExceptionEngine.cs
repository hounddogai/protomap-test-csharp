namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling laptop exception calculation.</summary>
public interface IRollingLaptopExceptionModifier
{
    string Describe();
    double Adjust(RollingLaptopException item, double valueValue);
}

public sealed record FlaggedRollingLaptopExceptionAdjuster(Registry Flags, string Flag, double Percent) : IRollingLaptopExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLaptopException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingLaptopExceptionAdjuster(double Percent) : IRollingLaptopExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingLaptopException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingLaptopExceptionAdjuster(double Minimum) : IRollingLaptopExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingLaptopException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingLaptopExceptionAdjuster(List<double> Bounds, List<double> Rates) : IRollingLaptopExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingLaptopException item, double valueValue)
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

/// <summary>Runs the rolling laptop exception steps in order and keeps a trace of each.</summary>
public sealed class RollingLaptopExceptionEngine(List<IRollingLaptopExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLaptopExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLaptopExceptionEngine DefaultRollingLaptopExceptionEngine(Registry flags)
    {
        return new RollingLaptopExceptionEngine(
            new List<IRollingLaptopExceptionModifier>
            {
                new FlaggedRollingLaptopExceptionAdjuster(flags, "rolling-laptop-exception-fallback", 7.7),
                new PercentRollingLaptopExceptionAdjuster(17.2),
                new MinimumRollingLaptopExceptionAdjuster(36.8),
                new TieredRollingLaptopExceptionAdjuster(
                    new List<double> { 2806.0, 6872.0 },
                    new List<double> { 1.13, 1.08 }),
            },
            new List<string>(),
            4.8,
            80368.4);
    }

    public double Price(RollingLaptopException item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingLaptopException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
