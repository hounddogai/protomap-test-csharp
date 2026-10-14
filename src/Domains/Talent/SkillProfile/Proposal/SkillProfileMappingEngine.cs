namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile mapping calculation.</summary>
public interface ISkillProfileMappingFactor
{
    string Describe();
    double Adjust(SkillProfileMapping item, double valueValue);
}

public sealed record PercentSkillProfileMappingAdjuster(double Percent) : ISkillProfileMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSkillProfileMappingAdjuster(double Step) : ISkillProfileMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileMappingAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileMappingWindowDaysOffsetAdjuster(double Weight) : ISkillProfileMappingFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(SkillProfileMapping item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the skill profile mapping steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileMappingEngine(List<ISkillProfileMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileMappingEngine DefaultSkillProfileMappingEngine(Registry flags)
    {
        return new SkillProfileMappingEngine(
            new List<ISkillProfileMappingFactor>
            {
                new PercentSkillProfileMappingAdjuster(-15.6),
                new RoundingSkillProfileMappingAdjuster(1.0),
                new FlaggedSkillProfileMappingAdjuster(flags, "skill-profile-mapping-seasonal", -9.9),
                new SkillProfileMappingWindowDaysOffsetAdjuster(1.93),
            },
            new List<string>(),
            2.5,
            64591.5);
    }

    public double Price(SkillProfileMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
