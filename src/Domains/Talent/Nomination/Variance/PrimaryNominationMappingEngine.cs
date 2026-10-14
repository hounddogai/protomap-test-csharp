namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary nomination mapping calculation.</summary>
public interface IPrimaryNominationMappingAdjuster
{
    string Describe();
    double Adjust(PrimaryNominationMapping item, double valueValue);
}

public sealed record PrimaryNominationMappingTenureMonthsOffsetAdjuster(double Weight) : IPrimaryNominationMappingAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PrimaryNominationMapping item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedPrimaryNominationMappingAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryNominationMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryNominationMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryNominationMappingAdjuster(double Step) : IPrimaryNominationMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryNominationMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimaryNominationMappingAdjuster(double Cap) : IPrimaryNominationMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryNominationMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPrimaryNominationMappingAdjuster(double Percent) : IPrimaryNominationMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryNominationMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary nomination mapping steps in order and keeps a trace of each.</summary>
public sealed class PrimaryNominationMappingEngine(List<IPrimaryNominationMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryNominationMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryNominationMappingEngine DefaultPrimaryNominationMappingEngine(Registry flags)
    {
        return new PrimaryNominationMappingEngine(
            new List<IPrimaryNominationMappingAdjuster>
            {
                new PrimaryNominationMappingTenureMonthsOffsetAdjuster(1.22),
                new FlaggedPrimaryNominationMappingAdjuster(flags, "primary-nomination-mapping-global", 2.8),
                new RoundingPrimaryNominationMappingAdjuster(0.5),
                new CapPrimaryNominationMappingAdjuster(31929.0),
                new PercentPrimaryNominationMappingAdjuster(-1.4),
            },
            new List<string>(),
            3.4,
            74845.0);
    }

    public double Run(PrimaryNominationMapping item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryNominationMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
