namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the skill profile estimate calculation.</summary>
public interface ISkillProfileEstimateStep
{
    string Describe();
    double Adjust(SkillProfileEstimate item, double valueValue);
}

public sealed record TieredSkillProfileEstimateAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
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

public sealed record FlaggedSkillProfileEstimateAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleSkillProfileEstimateAdjuster(Dictionary<string, double> Factors) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSkillProfileEstimateAdjuster(double Percent) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSkillProfileEstimateAdjuster(double Minimum) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SkillProfileEstimateMarginOffsetAdjuster(double Weight) : ISkillProfileEstimateStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SkillProfileEstimate item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the skill profile estimate steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileEstimateEngine(List<ISkillProfileEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileEstimateEngine DefaultSkillProfileEstimateEngine(Registry flags)
    {
        return new SkillProfileEstimateEngine(
            new List<ISkillProfileEstimateStep>
            {
                new TieredSkillProfileEstimateAdjuster(
                    new List<double> { 1891.0, 2416.0, 3777.0, 7491.0 },
                    new List<double> { 1.1, 0.8, 1.16, 1.15 }),
                new FlaggedSkillProfileEstimateAdjuster(flags, "skill-profile-estimate-weighted", 5.0),
                new LocaleSkillProfileEstimateAdjuster(new Dictionary<string, double> { ["retail"] = 1.38 }),
                new PercentSkillProfileEstimateAdjuster(-22.5),
                new MinimumSkillProfileEstimateAdjuster(44.1),
                new SkillProfileEstimateMarginOffsetAdjuster(0.58),
            },
            new List<string>(),
            2.4,
            76741.8);
    }

    public double Price(SkillProfileEstimate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
