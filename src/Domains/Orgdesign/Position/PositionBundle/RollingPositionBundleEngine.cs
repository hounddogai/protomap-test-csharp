namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling position bundle calculation.</summary>
public interface IRollingPositionBundleStep
{
    string Describe();
    double Adjust(RollingPositionBundle item, double valueValue);
}

public sealed record FlaggedRollingPositionBundleAdjuster(Registry Flags, string Flag, double Percent) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingPositionBundleAdjuster(double Step) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingPositionBundleAdjuster(double Minimum) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingPositionBundleAdjuster(double Cap) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingPositionBundleAdjuster(double Percent) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingPositionBundleCapacityOffsetAdjuster(double Weight) : IRollingPositionBundleStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(RollingPositionBundle item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the rolling position bundle steps in order and keeps a trace of each.</summary>
public sealed class RollingPositionBundleEngine(List<IRollingPositionBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPositionBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPositionBundleEngine DefaultRollingPositionBundleEngine(Registry flags)
    {
        return new RollingPositionBundleEngine(
            new List<IRollingPositionBundleStep>
            {
                new FlaggedRollingPositionBundleAdjuster(flags, "rolling-position-bundle-global", 6.8),
                new RoundingRollingPositionBundleAdjuster(0.1),
                new MinimumRollingPositionBundleAdjuster(40.7),
                new CapRollingPositionBundleAdjuster(49085.0),
                new PercentRollingPositionBundleAdjuster(-23.8),
                new RollingPositionBundleCapacityOffsetAdjuster(1.39),
            },
            new List<string>(),
            0.7,
            26857.1);
    }

    public double Compute(RollingPositionBundle item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPositionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
