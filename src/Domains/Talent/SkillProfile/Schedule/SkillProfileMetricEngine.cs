namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the skill profile metric calculation.</summary>
public interface ISkillProfileMetricStep
{
    string Describe();
    double Adjust(SkillProfileMetric item, double valueValue);
}

public sealed record PercentSkillProfileMetricAdjuster(double Percent) : ISkillProfileMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleSkillProfileMetricAdjuster(Dictionary<string, double> Factors) : ISkillProfileMetricStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SkillProfileMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSkillProfileMetricAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileMetricStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileMetricAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileMetric item, double valueValue)
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

/// <summary>Runs the skill profile metric steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileMetricEngine(List<ISkillProfileMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileMetricEngine DefaultSkillProfileMetricEngine(Registry flags)
    {
        return new SkillProfileMetricEngine(
            new List<ISkillProfileMetricStep>
            {
                new PercentSkillProfileMetricAdjuster(0.8),
                new LocaleSkillProfileMetricAdjuster(new Dictionary<string, double> { ["online"] = 0.73 }),
                new FlaggedSkillProfileMetricAdjuster(flags, "skill-profile-metric-adaptive", -8.0),
                new TieredSkillProfileMetricAdjuster(
                    new List<double> { 5473.0, 5539.0 },
                    new List<double> { 0.96, 0.89 }),
            },
            new List<string>(),
            3.8,
            67153.9);
    }

    public double Compute(SkillProfileMetric item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
