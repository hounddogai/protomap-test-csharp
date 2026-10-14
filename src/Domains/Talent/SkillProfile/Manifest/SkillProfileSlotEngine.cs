namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile slot calculation.</summary>
public interface ISkillProfileSlotModifier
{
    string Describe();
    double Adjust(SkillProfileSlot item, double valueValue);
}

public sealed record LabelSkillProfileSlotAdjuster(Dictionary<string, double> Factors) : ISkillProfileSlotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SkillProfileSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSkillProfileSlotAdjuster(double Percent) : ISkillProfileSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSkillProfileSlotAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileSlotUpdatedAtOffsetAdjuster(double Weight) : ISkillProfileSlotModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(SkillProfileSlot item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the skill profile slot steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileSlotEngine(List<ISkillProfileSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileSlotEngine DefaultSkillProfileSlotEngine(Registry flags)
    {
        return new SkillProfileSlotEngine(
            new List<ISkillProfileSlotModifier>
            {
                new LabelSkillProfileSlotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.83, ["online"] = 0.87 }),
                new PercentSkillProfileSlotAdjuster(11.7),
                new FlaggedSkillProfileSlotAdjuster(flags, "skill-profile-slot-seasonal", 9.0),
                new SkillProfileSlotUpdatedAtOffsetAdjuster(0.36),
            },
            new List<string>(),
            2.8,
            32834.7);
    }

    public double Price(SkillProfileSlot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
