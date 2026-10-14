namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled skill profile proposal calculation.</summary>
public interface IScheduledSkillProfileProposalFactor
{
    string Describe();
    double Adjust(ScheduledSkillProfileProposal item, double valueValue);
}

public sealed record RoundingScheduledSkillProfileProposalAdjuster(double Step) : IScheduledSkillProfileProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledSkillProfileProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredScheduledSkillProfileProposalAdjuster(List<double> Bounds, List<double> Rates) : IScheduledSkillProfileProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledSkillProfileProposal item, double valueValue)
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

public sealed record FlaggedScheduledSkillProfileProposalAdjuster(Registry Flags, string Flag, double Percent) : IScheduledSkillProfileProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledSkillProfileProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelScheduledSkillProfileProposalAdjuster(Dictionary<string, double> Factors) : IScheduledSkillProfileProposalFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledSkillProfileProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledSkillProfileProposalAdjuster(double Percent) : IScheduledSkillProfileProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledSkillProfileProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled skill profile proposal steps in order and keeps a trace of each.</summary>
public sealed class ScheduledSkillProfileProposalEngine(List<IScheduledSkillProfileProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledSkillProfileProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledSkillProfileProposalEngine DefaultScheduledSkillProfileProposalEngine(Registry flags)
    {
        return new ScheduledSkillProfileProposalEngine(
            new List<IScheduledSkillProfileProposalFactor>
            {
                new RoundingScheduledSkillProfileProposalAdjuster(5.0),
                new TieredScheduledSkillProfileProposalAdjuster(
                    new List<double> { 3302.0, 5871.0, 8097.0, 9472.0 },
                    new List<double> { 1.13, 1.14, 1.11, 1.02 }),
                new FlaggedScheduledSkillProfileProposalAdjuster(
                    flags,
                    "scheduled-skill-profile-proposal-batched",
                    1.3),
                new LabelScheduledSkillProfileProposalAdjuster(new Dictionary<string, double> { ["south"] = 0.78 }),
                new PercentScheduledSkillProfileProposalAdjuster(15.9),
            },
            new List<string>(),
            1.4,
            29707.5);
    }

    public double Run(ScheduledSkillProfileProposal item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledSkillProfileProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
