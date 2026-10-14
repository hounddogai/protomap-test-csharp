namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default headcount target calculation.</summary>
public interface IDefaultHeadcountTargetFactor
{
    string Describe();
    double Adjust(DefaultHeadcountTarget item, double valueValue);
}

public sealed record CapDefaultHeadcountTargetAdjuster(double Cap) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDefaultHeadcountTargetAdjuster(double Percent) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultHeadcountTargetAdjuster(double Step) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDefaultHeadcountTargetAdjuster(List<double> Bounds, List<double> Rates) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
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

public sealed record FlaggedDefaultHeadcountTargetAdjuster(Registry Flags, string Flag, double Percent) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultHeadcountTargetAdjuster(double Minimum) : IDefaultHeadcountTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultHeadcountTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default headcount target steps in order and keeps a trace of each.</summary>
public sealed class DefaultHeadcountTargetEngine(List<IDefaultHeadcountTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHeadcountTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultHeadcountTargetEngine DefaultDefaultHeadcountTargetEngine(Registry flags)
    {
        return new DefaultHeadcountTargetEngine(
            new List<IDefaultHeadcountTargetFactor>
            {
                new CapDefaultHeadcountTargetAdjuster(25608.0),
                new PercentDefaultHeadcountTargetAdjuster(22.6),
                new RoundingDefaultHeadcountTargetAdjuster(0.1),
                new TieredDefaultHeadcountTargetAdjuster(
                    new List<double> { 1048.0, 1558.0, 6215.0, 6870.0 },
                    new List<double> { 1.1, 1.04, 0.92, 0.86 }),
                new FlaggedDefaultHeadcountTargetAdjuster(flags, "default-headcount-target-global", -3.3),
                new MinimumDefaultHeadcountTargetAdjuster(13.8),
            },
            new List<string>(),
            2.3,
            29571.8);
    }

    public double Compute(DefaultHeadcountTarget item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHeadcountTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
