namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile segment calculation.</summary>
public interface ISkillProfileSegmentAdjuster
{
    string Describe();
    double Adjust(SkillProfileSegment item, double valueValue);
}

public sealed record MinimumSkillProfileSegmentAdjuster(double Minimum) : ISkillProfileSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSkillProfileSegmentAdjuster(double Step) : ISkillProfileSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileSegmentAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileSegmentRevisionOffsetAdjuster(double Weight) : ISkillProfileSegmentAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(SkillProfileSegment item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the skill profile segment steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileSegmentEngine(List<ISkillProfileSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileSegmentEngine DefaultSkillProfileSegmentEngine(Registry flags)
    {
        return new SkillProfileSegmentEngine(
            new List<ISkillProfileSegmentAdjuster>
            {
                new MinimumSkillProfileSegmentAdjuster(21.4),
                new RoundingSkillProfileSegmentAdjuster(0.1),
                new FlaggedSkillProfileSegmentAdjuster(flags, "skill-profile-segment-manual", 8.0),
                new SkillProfileSegmentRevisionOffsetAdjuster(1.07),
            },
            new List<string>(),
            4.9,
            97163.7);
    }

    public double Run(SkillProfileSegment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
