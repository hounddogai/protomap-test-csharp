namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive reorg snapshot calculation.</summary>
public interface IAdaptiveReorgSnapshotStep
{
    string Describe();
    double Adjust(AdaptiveReorgSnapshot item, double valueValue);
}

public sealed record CapAdaptiveReorgSnapshotAdjuster(double Cap) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptiveReorgSnapshotAdjuster(double Minimum) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveReorgSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
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

public sealed record RoundingAdaptiveReorgSnapshotAdjuster(double Step) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveReorgSnapshotAdjuster(double Percent) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveReorgSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveReorgSnapshotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveReorgSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive reorg snapshot steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveReorgSnapshotEngine(List<IAdaptiveReorgSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveReorgSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveReorgSnapshotEngine DefaultAdaptiveReorgSnapshotEngine(Registry flags)
    {
        return new AdaptiveReorgSnapshotEngine(
            new List<IAdaptiveReorgSnapshotStep>
            {
                new CapAdaptiveReorgSnapshotAdjuster(4418.0),
                new MinimumAdaptiveReorgSnapshotAdjuster(46.7),
                new TieredAdaptiveReorgSnapshotAdjuster(
                    new List<double> { 4515.0, 5306.0 },
                    new List<double> { 0.93, 1.05 }),
                new RoundingAdaptiveReorgSnapshotAdjuster(0.1),
                new PercentAdaptiveReorgSnapshotAdjuster(13.6),
                new FlaggedAdaptiveReorgSnapshotAdjuster(flags, "adaptive-reorg-snapshot-default", -1.7),
            },
            new List<string>(),
            2.6,
            93564.5);
    }

    public double Price(AdaptiveReorgSnapshot item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveReorgSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
