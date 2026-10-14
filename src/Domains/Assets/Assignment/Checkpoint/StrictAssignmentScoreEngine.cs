namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict assignment score calculation.</summary>
public interface IStrictAssignmentScoreAdjuster
{
    string Describe();
    double Adjust(StrictAssignmentScore item, double valueValue);
}

public sealed record TieredStrictAssignmentScoreAdjuster(List<double> Bounds, List<double> Rates) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
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

public sealed record MinimumStrictAssignmentScoreAdjuster(double Minimum) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictAssignmentScoreFloorValueOffsetAdjuster(double Weight) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapStrictAssignmentScoreAdjuster(double Cap) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictAssignmentScoreAdjuster(double Step) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStrictAssignmentScoreAdjuster(Registry Flags, string Flag, double Percent) : IStrictAssignmentScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictAssignmentScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict assignment score steps in order and keeps a trace of each.</summary>
public sealed class StrictAssignmentScoreEngine(List<IStrictAssignmentScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssignmentScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssignmentScoreEngine DefaultStrictAssignmentScoreEngine(Registry flags)
    {
        return new StrictAssignmentScoreEngine(
            new List<IStrictAssignmentScoreAdjuster>
            {
                new TieredStrictAssignmentScoreAdjuster(
                    new List<double> { 4083.0, 6975.0 },
                    new List<double> { 0.84, 1.14 }),
                new MinimumStrictAssignmentScoreAdjuster(49.2),
                new StrictAssignmentScoreFloorValueOffsetAdjuster(1.79),
                new CapStrictAssignmentScoreAdjuster(2759.0),
                new RoundingStrictAssignmentScoreAdjuster(0.05),
                new FlaggedStrictAssignmentScoreAdjuster(flags, "strict-assignment-score-weighted", 0.3),
            },
            new List<string>(),
            4.4,
            56588.9);
    }

    public double Price(StrictAssignmentScore item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssignmentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
