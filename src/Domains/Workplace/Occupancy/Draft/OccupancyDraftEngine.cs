namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy draft calculation.</summary>
public interface IOccupancyDraftFactor
{
    string Describe();
    double Adjust(OccupancyDraft item, double valueValue);
}

public sealed record TieredOccupancyDraftAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyDraft item, double valueValue)
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

public sealed record RoundingOccupancyDraftAdjuster(double Step) : IOccupancyDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedOccupancyDraftAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyDraftAdjuster(double Cap) : IOccupancyDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record OccupancyDraftScoreOffsetAdjuster(double Weight) : IOccupancyDraftFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(OccupancyDraft item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the occupancy draft steps in order and keeps a trace of each.</summary>
public sealed class OccupancyDraftEngine(List<IOccupancyDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyDraftEngine DefaultOccupancyDraftEngine(Registry flags)
    {
        return new OccupancyDraftEngine(
            new List<IOccupancyDraftFactor>
            {
                new TieredOccupancyDraftAdjuster(new List<double> { 3165.0, 7342.0 }, new List<double> { 1.02, 1.0 }),
                new RoundingOccupancyDraftAdjuster(0.5),
                new FlaggedOccupancyDraftAdjuster(flags, "occupancy-draft-tiered", -7.5),
                new CapOccupancyDraftAdjuster(27182.0),
                new OccupancyDraftScoreOffsetAdjuster(0.41),
            },
            new List<string>(),
            0.2,
            10881.9);
    }

    public double Price(OccupancyDraft item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
