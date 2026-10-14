namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the position score calculation.</summary>
public interface IPositionScoreFactor
{
    string Describe();
    double Adjust(PositionScore item, double valueValue);
}

public sealed record MinimumPositionScoreAdjuster(double Minimum) : IPositionScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPositionScoreAdjuster(double Cap) : IPositionScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NamePositionScoreAdjuster(Dictionary<string, double> Factors) : IPositionScoreFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PositionScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPositionScoreAdjuster(List<double> Bounds, List<double> Rates) : IPositionScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionScore item, double valueValue)
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

public sealed record PositionScoreQuantityOffsetAdjuster(double Weight) : IPositionScoreFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PositionScore item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedPositionScoreAdjuster(Registry Flags, string Flag, double Percent) : IPositionScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position score steps in order and keeps a trace of each.</summary>
public sealed class PositionScoreEngine(List<IPositionScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionScoreEngine DefaultPositionScoreEngine(Registry flags)
    {
        return new PositionScoreEngine(
            new List<IPositionScoreFactor>
            {
                new MinimumPositionScoreAdjuster(2.7),
                new CapPositionScoreAdjuster(26040.0),
                new NamePositionScoreAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.15, ["east"] = 0.98, ["central"] = 0.77 }),
                new TieredPositionScoreAdjuster(
                    new List<double> { 2228.0, 4106.0, 4822.0, 6722.0 },
                    new List<double> { 1.13, 0.86, 1.17, 1.15 }),
                new PositionScoreQuantityOffsetAdjuster(1.03),
                new FlaggedPositionScoreAdjuster(flags, "position-score-fallback", 7.8),
            },
            new List<string>(),
            3.3,
            45564.6);
    }

    public double Run(PositionScore item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
