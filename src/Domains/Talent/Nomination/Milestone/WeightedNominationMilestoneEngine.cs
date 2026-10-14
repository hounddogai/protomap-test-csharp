namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted nomination milestone calculation.</summary>
public interface IWeightedNominationMilestoneModifier
{
    string Describe();
    double Adjust(WeightedNominationMilestone item, double valueValue);
}

public sealed record FlaggedWeightedNominationMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedNominationMilestoneStartsAtOffsetAdjuster(double Weight) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CapWeightedNominationMilestoneAdjuster(double Cap) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedNominationMilestoneAdjuster(double Percent) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedNominationMilestoneAdjuster(double Step) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedNominationMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IWeightedNominationMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedNominationMilestone item, double valueValue)
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

/// <summary>Runs the weighted nomination milestone steps in order and keeps a trace of each.</summary>
public sealed class WeightedNominationMilestoneEngine(List<IWeightedNominationMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedNominationMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedNominationMilestoneEngine DefaultWeightedNominationMilestoneEngine(Registry flags)
    {
        return new WeightedNominationMilestoneEngine(
            new List<IWeightedNominationMilestoneModifier>
            {
                new FlaggedWeightedNominationMilestoneAdjuster(flags, "weighted-nomination-milestone-legacy", 3.6),
                new WeightedNominationMilestoneStartsAtOffsetAdjuster(1.97),
                new CapWeightedNominationMilestoneAdjuster(27282.0),
                new PercentWeightedNominationMilestoneAdjuster(10.5),
                new RoundingWeightedNominationMilestoneAdjuster(0.1),
                new TieredWeightedNominationMilestoneAdjuster(
                    new List<double> { 947.0, 3643.0, 5120.0 },
                    new List<double> { 1.07, 0.86, 1.01 }),
            },
            new List<string>(),
            4.5,
            70163.7);
    }

    public double Price(WeightedNominationMilestone item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedNominationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
