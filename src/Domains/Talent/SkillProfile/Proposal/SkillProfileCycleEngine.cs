namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile cycle calculation.</summary>
public interface ISkillProfileCycleModifier
{
    string Describe();
    double Adjust(SkillProfileCycle item, double valueValue);
}

public sealed record FlaggedSkillProfileCycleAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSkillProfileCycleAdjuster(double Percent) : ISkillProfileCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSkillProfileCycleAdjuster(double Cap) : ISkillProfileCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSkillProfileCycleAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileCycle item, double valueValue)
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

/// <summary>Runs the skill profile cycle steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileCycleEngine(List<ISkillProfileCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileCycleEngine DefaultSkillProfileCycleEngine(Registry flags)
    {
        return new SkillProfileCycleEngine(
            new List<ISkillProfileCycleModifier>
            {
                new FlaggedSkillProfileCycleAdjuster(flags, "skill-profile-cycle-legacy", 1.4),
                new PercentSkillProfileCycleAdjuster(-16.7),
                new CapSkillProfileCycleAdjuster(36183.0),
                new TieredSkillProfileCycleAdjuster(
                    new List<double> { 1290.0, 5258.0, 9221.0 },
                    new List<double> { 0.9, 0.93, 1.16 }),
            },
            new List<string>(),
            0.8,
            2266.2);
    }

    public double Compute(SkillProfileCycle item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
