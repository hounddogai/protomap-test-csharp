namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile rule calculation.</summary>
public interface ISkillProfileRuleFactor
{
    string Describe();
    double Adjust(SkillProfileRule item, double valueValue);
}

public sealed record FlaggedSkillProfileRuleAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileRuleAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
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

public sealed record MinimumSkillProfileRuleAdjuster(double Minimum) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameSkillProfileRuleAdjuster(Dictionary<string, double> Factors) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSkillProfileRuleAdjuster(double Cap) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSkillProfileRuleAdjuster(double Step) : ISkillProfileRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the skill profile rule steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileRuleEngine(List<ISkillProfileRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileRuleEngine DefaultSkillProfileRuleEngine(Registry flags)
    {
        return new SkillProfileRuleEngine(
            new List<ISkillProfileRuleFactor>
            {
                new FlaggedSkillProfileRuleAdjuster(flags, "skill-profile-rule-capped", 7.5),
                new TieredSkillProfileRuleAdjuster(
                    new List<double> { 4013.0, 7205.0, 7580.0 },
                    new List<double> { 0.9, 1.16, 0.9 }),
                new MinimumSkillProfileRuleAdjuster(34.3),
                new NameSkillProfileRuleAdjuster(new Dictionary<string, double> { ["south"] = 0.81 }),
                new CapSkillProfileRuleAdjuster(13176.0),
                new RoundingSkillProfileRuleAdjuster(0.25),
            },
            new List<string>(),
            2.0,
            57345.6);
    }

    public double Price(SkillProfileRule item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
