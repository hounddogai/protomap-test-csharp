namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment tier calculation.</summary>
public interface IAssignmentTierFactor
{
    string Describe();
    double Adjust(AssignmentTier item, double valueValue);
}

public sealed record TieredAssignmentTierAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentTier item, double valueValue)
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

public sealed record RegionAssignmentTierAdjuster(Dictionary<string, double> Factors) : IAssignmentTierFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AssignmentTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAssignmentTierAdjuster(double Step) : IAssignmentTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAssignmentTierAdjuster(double Percent) : IAssignmentTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AssignmentTierWeightOffsetAdjuster(double Weight) : IAssignmentTierFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(AssignmentTier item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedAssignmentTierAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment tier steps in order and keeps a trace of each.</summary>
public sealed class AssignmentTierEngine(List<IAssignmentTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentTierEngine DefaultAssignmentTierEngine(Registry flags)
    {
        return new AssignmentTierEngine(
            new List<IAssignmentTierFactor>
            {
                new TieredAssignmentTierAdjuster(
                    new List<double> { 157.0, 5292.0, 9932.0 },
                    new List<double> { 0.94, 0.97, 0.96 }),
                new RegionAssignmentTierAdjuster(new Dictionary<string, double> { ["central"] = 1.37 }),
                new RoundingAssignmentTierAdjuster(0.1),
                new PercentAssignmentTierAdjuster(-3.8),
                new AssignmentTierWeightOffsetAdjuster(1.61),
                new FlaggedAssignmentTierAdjuster(flags, "assignment-tier-rolling", 1.9),
            },
            new List<string>(),
            3.6,
            54300.6);
    }

    public double Evaluate(AssignmentTier item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
