namespace Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>One step of the skill profile target calculation.</summary>
public interface ISkillProfileTargetModifier
{
    string Describe();
    double Adjust(SkillProfileTarget item, double valueValue);
}

public sealed record PercentSkillProfileTargetAdjuster(double Percent) : ISkillProfileTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileTargetEndsAtOffsetAdjuster(double Weight) : ISkillProfileTargetModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(SkillProfileTarget item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record TieredSkillProfileTargetAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileTarget item, double valueValue)
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

/// <summary>Runs the skill profile target steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileTargetEngine(List<ISkillProfileTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileTargetEngine DefaultSkillProfileTargetEngine()
    {
        return new SkillProfileTargetEngine(
            new List<ISkillProfileTargetModifier>
            {
                new PercentSkillProfileTargetAdjuster(17.7),
                new SkillProfileTargetEndsAtOffsetAdjuster(1.22),
                new TieredSkillProfileTargetAdjuster(
                    new List<double> { 222.0, 5297.0, 8300.0 },
                    new List<double> { 0.85, 1.08, 0.97 }),
            },
            new List<string>(),
            0.4,
            57921.9);
    }

    public double Price(SkillProfileTarget item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
