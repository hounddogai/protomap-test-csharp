namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy town hall score calculation.</summary>
public interface ILegacyTownHallScoreModifier
{
    string Describe();
    double Adjust(LegacyTownHallScore item, double valueValue);
}

public sealed record RoundingLegacyTownHallScoreAdjuster(double Step) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyTownHallScoreAdjuster(List<double> Bounds, List<double> Rates) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
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

public sealed record MinimumLegacyTownHallScoreAdjuster(double Minimum) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LegacyTownHallScoreAttemptsOffsetAdjuster(double Weight) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record FlaggedLegacyTownHallScoreAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyTownHallScoreAdjuster(double Cap) : ILegacyTownHallScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTownHallScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy town hall score steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallScoreEngine(List<ILegacyTownHallScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallScoreEngine DefaultLegacyTownHallScoreEngine(Registry flags)
    {
        return new LegacyTownHallScoreEngine(
            new List<ILegacyTownHallScoreModifier>
            {
                new RoundingLegacyTownHallScoreAdjuster(0.1),
                new TieredLegacyTownHallScoreAdjuster(
                    new List<double> { 813.0, 5941.0, 7239.0 },
                    new List<double> { 1.17, 0.88, 0.89 }),
                new MinimumLegacyTownHallScoreAdjuster(8.7),
                new LegacyTownHallScoreAttemptsOffsetAdjuster(0.12),
                new FlaggedLegacyTownHallScoreAdjuster(flags, "legacy-town-hall-score-seasonal", 8.9),
                new CapLegacyTownHallScoreAdjuster(16825.0),
            },
            new List<string>(),
            5.0,
            48102.2);
    }

    public double Compute(LegacyTownHallScore item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
