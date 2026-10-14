namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary visitor override calculation.</summary>
public interface IPrimaryVisitorOverrideAdjuster
{
    string Describe();
    double Adjust(PrimaryVisitorOverride item, double valueValue);
}

public sealed record PercentPrimaryVisitorOverrideAdjuster(double Percent) : IPrimaryVisitorOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryVisitorOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimaryVisitorOverrideAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryVisitorOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryVisitorOverride item, double valueValue)
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

public sealed record FlaggedPrimaryVisitorOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryVisitorOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryVisitorOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryVisitorOverrideAdjuster(double Minimum) : IPrimaryVisitorOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryVisitorOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryVisitorOverrideAdjuster(double Cap) : IPrimaryVisitorOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryVisitorOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary visitor override steps in order and keeps a trace of each.</summary>
public sealed class PrimaryVisitorOverrideEngine(List<IPrimaryVisitorOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryVisitorOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryVisitorOverrideEngine DefaultPrimaryVisitorOverrideEngine(Registry flags)
    {
        return new PrimaryVisitorOverrideEngine(
            new List<IPrimaryVisitorOverrideAdjuster>
            {
                new PercentPrimaryVisitorOverrideAdjuster(-9.8),
                new TieredPrimaryVisitorOverrideAdjuster(
                    new List<double> { 94.0, 2452.0, 8424.0 },
                    new List<double> { 0.97, 1.15, 1.08 }),
                new FlaggedPrimaryVisitorOverrideAdjuster(flags, "primary-visitor-override-global", -8.4),
                new MinimumPrimaryVisitorOverrideAdjuster(33.7),
                new CapPrimaryVisitorOverrideAdjuster(8727.0),
            },
            new List<string>(),
            2.4,
            22168.1);
    }

    public double Run(PrimaryVisitorOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryVisitorOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
