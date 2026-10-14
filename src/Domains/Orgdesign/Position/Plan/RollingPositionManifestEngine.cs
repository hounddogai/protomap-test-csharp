namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling position manifest calculation.</summary>
public interface IRollingPositionManifestStep
{
    string Describe();
    double Adjust(RollingPositionManifest item, double valueValue);
}

public sealed record CapRollingPositionManifestAdjuster(double Cap) : IRollingPositionManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPositionManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingPositionManifestAdjuster(Registry Flags, string Flag, double Percent) : IRollingPositionManifestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPositionManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentRollingPositionManifestAdjuster(Dictionary<string, double> Factors) : IRollingPositionManifestStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RollingPositionManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingPositionManifestAdjuster(double Percent) : IRollingPositionManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPositionManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling position manifest steps in order and keeps a trace of each.</summary>
public sealed class RollingPositionManifestEngine(List<IRollingPositionManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPositionManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPositionManifestEngine DefaultRollingPositionManifestEngine(Registry flags)
    {
        return new RollingPositionManifestEngine(
            new List<IRollingPositionManifestStep>
            {
                new CapRollingPositionManifestAdjuster(2447.0),
                new FlaggedRollingPositionManifestAdjuster(flags, "rolling-position-manifest-tiered", -8.1),
                new SegmentRollingPositionManifestAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.75, ["west"] = 1.37 }),
                new PercentRollingPositionManifestAdjuster(21.7),
            },
            new List<string>(),
            2.3,
            75782.8);
    }

    public double Evaluate(RollingPositionManifest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPositionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
