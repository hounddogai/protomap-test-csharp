namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination milestone calculation.</summary>
public interface INominationMilestoneStep
{
    string Describe();
    double Adjust(NominationMilestone item, double valueValue);
}

public sealed record NominationMilestoneGraceDaysOffsetAdjuster(double Weight) : INominationMilestoneStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(NominationMilestone item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record FlaggedNominationMilestoneAdjuster(Registry Flags, string Flag, double Percent) : INominationMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentNominationMilestoneAdjuster(double Percent) : INominationMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationMilestoneAdjuster(List<double> Bounds, List<double> Rates) : INominationMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationMilestone item, double valueValue)
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

public sealed record CapNominationMilestoneAdjuster(double Cap) : INominationMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the nomination milestone steps in order and keeps a trace of each.</summary>
public sealed class NominationMilestoneEngine(List<INominationMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationMilestoneEngine DefaultNominationMilestoneEngine(Registry flags)
    {
        return new NominationMilestoneEngine(
            new List<INominationMilestoneStep>
            {
                new NominationMilestoneGraceDaysOffsetAdjuster(1.59),
                new FlaggedNominationMilestoneAdjuster(flags, "nomination-milestone-adaptive", -5.4),
                new PercentNominationMilestoneAdjuster(-17.3),
                new TieredNominationMilestoneAdjuster(
                    new List<double> { 1692.0, 3248.0, 5776.0, 7924.0 },
                    new List<double> { 1.18, 1.11, 1.17, 0.95 }),
                new CapNominationMilestoneAdjuster(39965.0),
            },
            new List<string>(),
            3.0,
            41118.4);
    }

    public double Price(NominationMilestone item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<NominationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
