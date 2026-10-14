namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position bundle calculation.</summary>
public interface IPositionBundleStep
{
    string Describe();
    double Adjust(PositionBundle item, double valueValue);
}

public sealed record MinimumPositionBundleAdjuster(double Minimum) : IPositionBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPositionBundleAdjuster(Registry Flags, string Flag, double Percent) : IPositionBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPositionBundleAdjuster(List<double> Bounds, List<double> Rates) : IPositionBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionBundle item, double valueValue)
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

public sealed record DepartmentPositionBundleAdjuster(Dictionary<string, double> Factors) : IPositionBundleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PositionBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPositionBundleAdjuster(double Step) : IPositionBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the position bundle steps in order and keeps a trace of each.</summary>
public sealed class PositionBundleEngine(List<IPositionBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionBundleEngine DefaultPositionBundleEngine(Registry flags)
    {
        return new PositionBundleEngine(
            new List<IPositionBundleStep>
            {
                new MinimumPositionBundleAdjuster(37.4),
                new FlaggedPositionBundleAdjuster(flags, "position-bundle-capped", 1.5),
                new TieredPositionBundleAdjuster(
                    new List<double> { 1266.0, 2540.0, 3305.0 },
                    new List<double> { 0.96, 0.97, 1.17 }),
                new DepartmentPositionBundleAdjuster(new Dictionary<string, double> { ["central"] = 1.03 }),
                new RoundingPositionBundleAdjuster(0.05),
            },
            new List<string>(),
            1.7,
            3687.7);
    }

    public double Run(PositionBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
