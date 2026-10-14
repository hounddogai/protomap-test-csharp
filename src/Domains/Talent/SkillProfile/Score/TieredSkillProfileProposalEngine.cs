namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered skill profile proposal calculation.</summary>
public interface ITieredSkillProfileProposalModifier
{
    string Describe();
    double Adjust(TieredSkillProfileProposal item, double valueValue);
}

public sealed record PercentTieredSkillProfileProposalAdjuster(double Percent) : ITieredSkillProfileProposalModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredSkillProfileProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTieredSkillProfileProposalAdjuster(Registry Flags, string Flag, double Percent) : ITieredSkillProfileProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSkillProfileProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredSkillProfileProposalAdjuster(double Cap) : ITieredSkillProfileProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredSkillProfileProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTieredSkillProfileProposalAdjuster(double Minimum) : ITieredSkillProfileProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSkillProfileProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered skill profile proposal steps in order and keeps a trace of each.</summary>
public sealed class TieredSkillProfileProposalEngine(List<ITieredSkillProfileProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSkillProfileProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSkillProfileProposalEngine DefaultTieredSkillProfileProposalEngine(Registry flags)
    {
        return new TieredSkillProfileProposalEngine(
            new List<ITieredSkillProfileProposalModifier>
            {
                new PercentTieredSkillProfileProposalAdjuster(2.1),
                new FlaggedTieredSkillProfileProposalAdjuster(flags, "tiered-skill-profile-proposal-adaptive", -5.5),
                new CapTieredSkillProfileProposalAdjuster(13113.0),
                new MinimumTieredSkillProfileProposalAdjuster(37.7),
            },
            new List<string>(),
            2.9,
            17743.8);
    }

    public double Run(TieredSkillProfileProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredSkillProfileProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
