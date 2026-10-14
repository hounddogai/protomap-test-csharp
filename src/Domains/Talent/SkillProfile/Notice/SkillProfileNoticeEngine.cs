namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile notice calculation.</summary>
public interface ISkillProfileNoticeModifier
{
    string Describe();
    double Adjust(SkillProfileNotice item, double valueValue);
}

public sealed record PercentSkillProfileNoticeAdjuster(double Percent) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileNoticeAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
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

public sealed record SkillProfileNoticeScoreOffsetAdjuster(double Weight) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TeamSkillProfileNoticeAdjuster(Dictionary<string, double> Factors) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSkillProfileNoticeAdjuster(double Step) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileNoticeAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the skill profile notice steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileNoticeEngine(List<ISkillProfileNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileNoticeEngine DefaultSkillProfileNoticeEngine(Registry flags)
    {
        return new SkillProfileNoticeEngine(
            new List<ISkillProfileNoticeModifier>
            {
                new PercentSkillProfileNoticeAdjuster(-19.1),
                new TieredSkillProfileNoticeAdjuster(
                    new List<double> { 1108.0, 2422.0, 7717.0 },
                    new List<double> { 1.08, 0.94, 0.86 }),
                new SkillProfileNoticeScoreOffsetAdjuster(1.17),
                new TeamSkillProfileNoticeAdjuster(new Dictionary<string, double> { ["retail"] = 0.97 }),
                new RoundingSkillProfileNoticeAdjuster(0.5),
                new FlaggedSkillProfileNoticeAdjuster(flags, "skill-profile-notice-capped", 4.9),
            },
            new List<string>(),
            2.2,
            66320.7);
    }

    public double Evaluate(SkillProfileNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
