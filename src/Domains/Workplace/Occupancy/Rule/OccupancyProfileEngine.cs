namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy profile calculation.</summary>
public interface IOccupancyProfileStep
{
    string Describe();
    double Adjust(OccupancyProfile item, double valueValue);
}

public sealed record PercentOccupancyProfileAdjuster(double Percent) : IOccupancyProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredOccupancyProfileAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyProfile item, double valueValue)
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

public sealed record CapOccupancyProfileAdjuster(double Cap) : IOccupancyProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedOccupancyProfileAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy profile steps in order and keeps a trace of each.</summary>
public sealed class OccupancyProfileEngine(List<IOccupancyProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyProfileEngine DefaultOccupancyProfileEngine(Registry flags)
    {
        return new OccupancyProfileEngine(
            new List<IOccupancyProfileStep>
            {
                new PercentOccupancyProfileAdjuster(-7.0),
                new TieredOccupancyProfileAdjuster(
                    new List<double> { 1735.0, 3951.0, 9262.0, 9576.0 },
                    new List<double> { 1.06, 1.19, 0.88, 0.88 }),
                new CapOccupancyProfileAdjuster(1792.0),
                new FlaggedOccupancyProfileAdjuster(flags, "occupancy-profile-lenient", -3.0),
            },
            new List<string>(),
            1.5,
            29324.6);
    }

    public double Run(OccupancyProfile item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
