namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary occupancy manifest calculation.</summary>
public interface IPrimaryOccupancyManifestModifier
{
    string Describe();
    double Adjust(PrimaryOccupancyManifest item, double valueValue);
}

public sealed record RoundingPrimaryOccupancyManifestAdjuster(double Step) : IPrimaryOccupancyManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryOccupancyManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryOccupancyManifestAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryOccupancyManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryOccupancyManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourcePrimaryOccupancyManifestAdjuster(Dictionary<string, double> Factors) : IPrimaryOccupancyManifestModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PrimaryOccupancyManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryOccupancyManifestAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryOccupancyManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryOccupancyManifest item, double valueValue)
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

public sealed record PrimaryOccupancyManifestLeaveDaysOffsetAdjuster(double Weight) : IPrimaryOccupancyManifestModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PrimaryOccupancyManifest item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the primary occupancy manifest steps in order and keeps a trace of each.</summary>
public sealed class PrimaryOccupancyManifestEngine(List<IPrimaryOccupancyManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryOccupancyManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryOccupancyManifestEngine DefaultPrimaryOccupancyManifestEngine(Registry flags)
    {
        return new PrimaryOccupancyManifestEngine(
            new List<IPrimaryOccupancyManifestModifier>
            {
                new RoundingPrimaryOccupancyManifestAdjuster(1.0),
                new FlaggedPrimaryOccupancyManifestAdjuster(flags, "primary-occupancy-manifest-default", 2.7),
                new SourcePrimaryOccupancyManifestAdjuster(new Dictionary<string, double> { ["online"] = 0.9 }),
                new TieredPrimaryOccupancyManifestAdjuster(
                    new List<double> { 540.0, 2836.0, 7200.0, 9371.0 },
                    new List<double> { 1.08, 1.03, 1.18, 0.86 }),
                new PrimaryOccupancyManifestLeaveDaysOffsetAdjuster(0.19),
            },
            new List<string>(),
            3.7,
            32985.0);
    }

    public double Evaluate(PrimaryOccupancyManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryOccupancyManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
