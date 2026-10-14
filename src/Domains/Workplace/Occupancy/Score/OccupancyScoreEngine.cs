namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy score calculation.</summary>
public interface IOccupancyScoreFactor
{
    string Describe();
    double Adjust(OccupancyScore item, double valueValue);
}

public sealed record CapOccupancyScoreAdjuster(double Cap) : IOccupancyScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedOccupancyScoreAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record OccupancyScoreSequenceOffsetAdjuster(double Weight) : IOccupancyScoreFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(OccupancyScore item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record NameOccupancyScoreAdjuster(Dictionary<string, double> Factors) : IOccupancyScoreFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OccupancyScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredOccupancyScoreAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyScore item, double valueValue)
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

/// <summary>Runs the occupancy score steps in order and keeps a trace of each.</summary>
public sealed class OccupancyScoreEngine(List<IOccupancyScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyScoreEngine DefaultOccupancyScoreEngine(Registry flags)
    {
        return new OccupancyScoreEngine(
            new List<IOccupancyScoreFactor>
            {
                new CapOccupancyScoreAdjuster(25792.0),
                new FlaggedOccupancyScoreAdjuster(flags, "occupancy-score-legacy", -7.6),
                new OccupancyScoreSequenceOffsetAdjuster(1.05),
                new NameOccupancyScoreAdjuster(new Dictionary<string, double> { ["retail"] = 1.18 }),
                new TieredOccupancyScoreAdjuster(
                    new List<double> { 4965.0, 5615.0, 9289.0, 9319.0 },
                    new List<double> { 1.03, 1.0, 0.83, 1.14 }),
            },
            new List<string>(),
            1.6,
            78294.4);
    }

    public double Evaluate(OccupancyScore item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
