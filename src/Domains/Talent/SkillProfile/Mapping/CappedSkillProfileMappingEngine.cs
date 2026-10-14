namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped skill profile mapping calculation.</summary>
public interface ICappedSkillProfileMappingStep
{
    string Describe();
    double Adjust(CappedSkillProfileMapping item, double valueValue);
}

public sealed record CategoryCappedSkillProfileMappingAdjuster(Dictionary<string, double> Factors) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedSkillProfileMappingAdjuster(double Step) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedSkillProfileMappingAdjuster(List<double> Bounds, List<double> Rates) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
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

public sealed record FlaggedCappedSkillProfileMappingAdjuster(Registry Flags, string Flag, double Percent) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedSkillProfileMappingAdjuster(double Minimum) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedSkillProfileMappingDiscountOffsetAdjuster(double Weight) : ICappedSkillProfileMappingStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(CappedSkillProfileMapping item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the capped skill profile mapping steps in order and keeps a trace of each.</summary>
public sealed class CappedSkillProfileMappingEngine(List<ICappedSkillProfileMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSkillProfileMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSkillProfileMappingEngine DefaultCappedSkillProfileMappingEngine(Registry flags)
    {
        return new CappedSkillProfileMappingEngine(
            new List<ICappedSkillProfileMappingStep>
            {
                new CategoryCappedSkillProfileMappingAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.19, ["west"] = 0.93, ["east"] = 1.37 }),
                new RoundingCappedSkillProfileMappingAdjuster(5.0),
                new TieredCappedSkillProfileMappingAdjuster(
                    new List<double> { 5927.0, 9809.0 },
                    new List<double> { 0.98, 1.17 }),
                new FlaggedCappedSkillProfileMappingAdjuster(flags, "capped-skill-profile-mapping-manual", -7.1),
                new MinimumCappedSkillProfileMappingAdjuster(24.1),
                new CappedSkillProfileMappingDiscountOffsetAdjuster(0.65),
            },
            new List<string>(),
            2.7,
            1932.9);
    }

    public double Price(CappedSkillProfileMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedSkillProfileMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
