namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy skill profile proposal calculation.</summary>
public interface ILegacySkillProfileProposalStep
{
    string Describe();
    double Adjust(LegacySkillProfileProposal item, double valueValue);
}

public sealed record TitleLegacySkillProfileProposalAdjuster(Dictionary<string, double> Factors) : ILegacySkillProfileProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacySkillProfileProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacySkillProfileProposalAdjuster(double Step) : ILegacySkillProfileProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacySkillProfileProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacySkillProfileProposalMaximumOffsetAdjuster(double Weight) : ILegacySkillProfileProposalStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LegacySkillProfileProposal item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedLegacySkillProfileProposalAdjuster(Registry Flags, string Flag, double Percent) : ILegacySkillProfileProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacySkillProfileProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy skill profile proposal steps in order and keeps a trace of each.</summary>
public sealed class LegacySkillProfileProposalEngine(List<ILegacySkillProfileProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacySkillProfileProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacySkillProfileProposalEngine DefaultLegacySkillProfileProposalEngine(Registry flags)
    {
        return new LegacySkillProfileProposalEngine(
            new List<ILegacySkillProfileProposalStep>
            {
                new TitleLegacySkillProfileProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.04, ["south"] = 0.8 }),
                new RoundingLegacySkillProfileProposalAdjuster(0.25),
                new LegacySkillProfileProposalMaximumOffsetAdjuster(1.0),
                new FlaggedLegacySkillProfileProposalAdjuster(flags, "legacy-skill-profile-proposal-pending", -2.1),
            },
            new List<string>(),
            0.4,
            48293.2);
    }

    public double Compute(LegacySkillProfileProposal item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacySkillProfileProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
