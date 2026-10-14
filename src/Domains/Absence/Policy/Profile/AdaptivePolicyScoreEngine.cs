namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive policy score calculation.</summary>
public interface IAdaptivePolicyScoreAdjuster
{
    string Describe();
    double Adjust(AdaptivePolicyScore item, double valueValue);
}

public sealed record AdaptivePolicyScoreUpdatedAtOffsetAdjuster(double Weight) : IAdaptivePolicyScoreAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AdaptivePolicyScore item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredAdaptivePolicyScoreAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePolicyScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePolicyScore item, double valueValue)
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

public sealed record FlaggedAdaptivePolicyScoreAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptivePolicyScoreAdjuster(double Cap) : IAdaptivePolicyScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePolicyScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive policy score steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyScoreEngine(List<IAdaptivePolicyScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyScoreEngine DefaultAdaptivePolicyScoreEngine(Registry flags)
    {
        return new AdaptivePolicyScoreEngine(
            new List<IAdaptivePolicyScoreAdjuster>
            {
                new AdaptivePolicyScoreUpdatedAtOffsetAdjuster(0.34),
                new TieredAdaptivePolicyScoreAdjuster(
                    new List<double> { 2480.0, 3653.0, 6410.0, 9070.0 },
                    new List<double> { 0.92, 0.94, 1.0, 1.02 }),
                new FlaggedAdaptivePolicyScoreAdjuster(flags, "adaptive-policy-score-rolling", -0.6),
                new CapAdaptivePolicyScoreAdjuster(44428.0),
            },
            new List<string>(),
            1.0,
            89135.5);
    }

    public double Evaluate(AdaptivePolicyScore item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
