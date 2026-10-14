namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional desk mapping calculation.</summary>
public interface IRegionalDeskMappingFactor
{
    string Describe();
    double Adjust(RegionalDeskMapping item, double valueValue);
}

public sealed record RegionalDeskMappingLimitOffsetAdjuster(double Weight) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapRegionalDeskMappingAdjuster(double Cap) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalDeskMappingAdjuster(double Step) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalDeskMappingAdjuster(List<double> Bounds, List<double> Rates) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
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

public sealed record PercentRegionalDeskMappingAdjuster(double Percent) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalDeskMappingAdjuster(Registry Flags, string Flag, double Percent) : IRegionalDeskMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalDeskMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional desk mapping steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeskMappingEngine(List<IRegionalDeskMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeskMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeskMappingEngine DefaultRegionalDeskMappingEngine(Registry flags)
    {
        return new RegionalDeskMappingEngine(
            new List<IRegionalDeskMappingFactor>
            {
                new RegionalDeskMappingLimitOffsetAdjuster(0.84),
                new CapRegionalDeskMappingAdjuster(14552.0),
                new RoundingRegionalDeskMappingAdjuster(5.0),
                new TieredRegionalDeskMappingAdjuster(
                    new List<double> { 3865.0, 7357.0, 8310.0, 9846.0 },
                    new List<double> { 0.88, 0.82, 1.13, 0.94 }),
                new PercentRegionalDeskMappingAdjuster(-23.9),
                new FlaggedRegionalDeskMappingAdjuster(flags, "regional-desk-mapping-weighted", 7.4),
            },
            new List<string>(),
            4.6,
            79449.1);
    }

    public double Run(RegionalDeskMapping item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalDeskMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
