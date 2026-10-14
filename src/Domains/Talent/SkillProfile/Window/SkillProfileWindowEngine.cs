namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the skill profile window calculation.</summary>
public interface ISkillProfileWindowModifier
{
    string Describe();
    double Adjust(SkillProfileWindow item, double valueValue);
}

public sealed record FlaggedSkillProfileWindowAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentSkillProfileWindowAdjuster(Dictionary<string, double> Factors) : ISkillProfileWindowModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(SkillProfileWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSkillProfileWindowAdjuster(double Minimum) : ISkillProfileWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSkillProfileWindowAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileWindow item, double valueValue)
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

public sealed record SkillProfileWindowScoreOffsetAdjuster(double Weight) : ISkillProfileWindowModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(SkillProfileWindow item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the skill profile window steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileWindowEngine(List<ISkillProfileWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileWindowEngine DefaultSkillProfileWindowEngine(Registry flags)
    {
        return new SkillProfileWindowEngine(
            new List<ISkillProfileWindowModifier>
            {
                new FlaggedSkillProfileWindowAdjuster(flags, "skill-profile-window-default", -4.0),
                new SegmentSkillProfileWindowAdjuster(new Dictionary<string, double> { ["east"] = 0.88 }),
                new MinimumSkillProfileWindowAdjuster(45.5),
                new TieredSkillProfileWindowAdjuster(
                    new List<double> { 2211.0, 3650.0, 9399.0 },
                    new List<double> { 1.1, 1.11, 0.95 }),
                new SkillProfileWindowScoreOffsetAdjuster(0.35),
            },
            new List<string>(),
            4.9,
            53725.3);
    }

    public double Price(SkillProfileWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
