namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile tier calculation.</summary>
public interface ISkillProfileTierFactor
{
    string Describe();
    double Adjust(SkillProfileTier item, double valueValue);
}

public sealed record CategorySkillProfileTierAdjuster(Dictionary<string, double> Factors) : ISkillProfileTierFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SkillProfileTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSkillProfileTierAdjuster(double Cap) : ISkillProfileTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSkillProfileTierAdjuster(double Step) : ISkillProfileTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSkillProfileTierAdjuster(double Minimum) : ISkillProfileTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the skill profile tier steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileTierEngine(List<ISkillProfileTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileTierEngine DefaultSkillProfileTierEngine()
    {
        return new SkillProfileTierEngine(
            new List<ISkillProfileTierFactor>
            {
                new CategorySkillProfileTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.86, ["west"] = 1.22, ["south"] = 1.06 }),
                new CapSkillProfileTierAdjuster(3311.0),
                new RoundingSkillProfileTierAdjuster(0.5),
                new MinimumSkillProfileTierAdjuster(3.9),
            },
            new List<string>(),
            3.8,
            96843.5);
    }

    public double Price(SkillProfileTier item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
