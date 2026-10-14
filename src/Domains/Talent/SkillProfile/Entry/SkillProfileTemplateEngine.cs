namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the skill profile template calculation.</summary>
public interface ISkillProfileTemplateStep
{
    string Describe();
    double Adjust(SkillProfileTemplate item, double valueValue);
}

public sealed record SkillProfileTemplateCostOffsetAdjuster(double Weight) : ISkillProfileTemplateStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SkillProfileTemplate item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentSkillProfileTemplateAdjuster(double Percent) : ISkillProfileTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSkillProfileTemplateAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelSkillProfileTemplateAdjuster(Dictionary<string, double> Factors) : ISkillProfileTemplateStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SkillProfileTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the skill profile template steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileTemplateEngine(List<ISkillProfileTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileTemplateEngine DefaultSkillProfileTemplateEngine(Registry flags)
    {
        return new SkillProfileTemplateEngine(
            new List<ISkillProfileTemplateStep>
            {
                new SkillProfileTemplateCostOffsetAdjuster(0.63),
                new PercentSkillProfileTemplateAdjuster(-10.5),
                new FlaggedSkillProfileTemplateAdjuster(flags, "skill-profile-template-strict", -8.7),
                new LabelSkillProfileTemplateAdjuster(new Dictionary<string, double> { ["west"] = 0.76 }),
            },
            new List<string>(),
            2.2,
            10691.7);
    }

    public double Evaluate(SkillProfileTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
