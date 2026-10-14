namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility score calculation.</summary>
public interface IMobilityScoreModifier
{
    string Describe();
    double Adjust(MobilityScore item, double valueValue);
}

public sealed record FlaggedMobilityScoreAdjuster(Registry Flags, string Flag, double Percent) : IMobilityScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMobilityScoreAdjuster(List<double> Bounds, List<double> Rates) : IMobilityScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityScore item, double valueValue)
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

public sealed record MobilityScoreLimitOffsetAdjuster(double Weight) : IMobilityScoreModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(MobilityScore item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the mobility score steps in order and keeps a trace of each.</summary>
public sealed class MobilityScoreEngine(List<IMobilityScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityScoreEngine DefaultMobilityScoreEngine(Registry flags)
    {
        return new MobilityScoreEngine(
            new List<IMobilityScoreModifier>
            {
                new FlaggedMobilityScoreAdjuster(flags, "mobility-score-default", 4.5),
                new TieredMobilityScoreAdjuster(
                    new List<double> { 502.0, 1640.0, 8081.0 },
                    new List<double> { 1.05, 0.82, 1.14 }),
                new MobilityScoreLimitOffsetAdjuster(0.84),
            },
            new List<string>(),
            3.4,
            21272.8);
    }

    public double Price(MobilityScore item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
