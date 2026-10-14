namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination mapping calculation.</summary>
public interface INominationMappingFactor
{
    string Describe();
    double Adjust(NominationMapping item, double valueValue);
}

public sealed record PercentNominationMappingAdjuster(double Percent) : INominationMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationMappingAdjuster(List<double> Bounds, List<double> Rates) : INominationMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationMapping item, double valueValue)
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

public sealed record CapNominationMappingAdjuster(double Cap) : INominationMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NominationMappingSequenceOffsetAdjuster(double Weight) : INominationMappingFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(NominationMapping item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedNominationMappingAdjuster(Registry Flags, string Flag, double Percent) : INominationMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination mapping steps in order and keeps a trace of each.</summary>
public sealed class NominationMappingEngine(List<INominationMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationMappingEngine DefaultNominationMappingEngine(Registry flags)
    {
        return new NominationMappingEngine(
            new List<INominationMappingFactor>
            {
                new PercentNominationMappingAdjuster(1.6),
                new TieredNominationMappingAdjuster(
                    new List<double> { 545.0, 6973.0 },
                    new List<double> { 0.83, 0.85 }),
                new CapNominationMappingAdjuster(10311.0),
                new NominationMappingSequenceOffsetAdjuster(1.05),
                new FlaggedNominationMappingAdjuster(flags, "nomination-mapping-regional", -7.5),
            },
            new List<string>(),
            3.0,
            86644.9);
    }

    public double Price(NominationMapping item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
