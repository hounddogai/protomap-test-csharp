namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy manifest calculation.</summary>
public interface IOccupancyManifestStep
{
    string Describe();
    double Adjust(OccupancyManifest item, double valueValue);
}

public sealed record TieredOccupancyManifestAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyManifest item, double valueValue)
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

public sealed record CategoryOccupancyManifestAdjuster(Dictionary<string, double> Factors) : IOccupancyManifestStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(OccupancyManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedOccupancyManifestAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyManifestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyManifestAdjuster(double Cap) : IOccupancyManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the occupancy manifest steps in order and keeps a trace of each.</summary>
public sealed class OccupancyManifestEngine(List<IOccupancyManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyManifestEngine DefaultOccupancyManifestEngine(Registry flags)
    {
        return new OccupancyManifestEngine(
            new List<IOccupancyManifestStep>
            {
                new TieredOccupancyManifestAdjuster(
                    new List<double> { 724.0, 6793.0 },
                    new List<double> { 1.12, 0.98 }),
                new CategoryOccupancyManifestAdjuster(new Dictionary<string, double> { ["west"] = 0.8 }),
                new FlaggedOccupancyManifestAdjuster(flags, "occupancy-manifest-capped", 8.2),
                new CapOccupancyManifestAdjuster(30502.0),
            },
            new List<string>(),
            3.8,
            55467.8);
    }

    public double Run(OccupancyManifest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
