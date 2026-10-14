namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy template calculation.</summary>
public interface IOccupancyTemplateModifier
{
    string Describe();
    double Adjust(OccupancyTemplate item, double valueValue);
}

public sealed record MinimumOccupancyTemplateAdjuster(double Minimum) : IOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyTemplateAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentOccupancyTemplateAdjuster(double Percent) : IOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyTemplateAdjuster(double Cap) : IOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredOccupancyTemplateAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyTemplate item, double valueValue)
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

/// <summary>Runs the occupancy template steps in order and keeps a trace of each.</summary>
public sealed class OccupancyTemplateEngine(List<IOccupancyTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyTemplateEngine DefaultOccupancyTemplateEngine(Registry flags)
    {
        return new OccupancyTemplateEngine(
            new List<IOccupancyTemplateModifier>
            {
                new MinimumOccupancyTemplateAdjuster(36.1),
                new FlaggedOccupancyTemplateAdjuster(flags, "occupancy-template-capped", -3.5),
                new PercentOccupancyTemplateAdjuster(-22.1),
                new CapOccupancyTemplateAdjuster(49332.0),
                new TieredOccupancyTemplateAdjuster(
                    new List<double> { 4610.0, 8301.0, 9323.0 },
                    new List<double> { 1.14, 1.08, 1.16 }),
            },
            new List<string>(),
            4.7,
            51479.9);
    }

    public double Run(OccupancyTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
