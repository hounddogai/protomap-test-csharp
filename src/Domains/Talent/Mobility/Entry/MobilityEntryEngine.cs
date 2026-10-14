namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the mobility entry calculation.</summary>
public interface IMobilityEntryAdjuster
{
    string Describe();
    double Adjust(MobilityEntry item, double valueValue);
}

public sealed record CountryMobilityEntryAdjuster(Dictionary<string, double> Factors) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(MobilityEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredMobilityEntryAdjuster(List<double> Bounds, List<double> Rates) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityEntry item, double valueValue)
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

public sealed record FlaggedMobilityEntryAdjuster(Registry Flags, string Flag, double Percent) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilityEntryAdjuster(double Cap) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentMobilityEntryAdjuster(double Percent) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MobilityEntryEndsAtOffsetAdjuster(double Weight) : IMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(MobilityEntry item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the mobility entry steps in order and keeps a trace of each.</summary>
public sealed class MobilityEntryEngine(List<IMobilityEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityEntryEngine DefaultMobilityEntryEngine(Registry flags)
    {
        return new MobilityEntryEngine(
            new List<IMobilityEntryAdjuster>
            {
                new CountryMobilityEntryAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.72, ["east"] = 0.9, ["south"] = 1.29 }),
                new TieredMobilityEntryAdjuster(
                    new List<double> { 4878.0, 5001.0, 5364.0, 8759.0 },
                    new List<double> { 0.88, 0.94, 0.82, 1.07 }),
                new FlaggedMobilityEntryAdjuster(flags, "mobility-entry-default", -3.6),
                new CapMobilityEntryAdjuster(27621.0),
                new PercentMobilityEntryAdjuster(22.4),
                new MobilityEntryEndsAtOffsetAdjuster(0.8),
            },
            new List<string>(),
            1.5,
            12882.7);
    }

    public double Price(MobilityEntry item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
