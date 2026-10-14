namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>One step of the skill profile quota calculation.</summary>
public interface ISkillProfileQuotaModifier
{
    string Describe();
    double Adjust(SkillProfileQuota item, double valueValue);
}

public sealed record TieredSkillProfileQuotaAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileQuota item, double valueValue)
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

public sealed record LabelSkillProfileQuotaAdjuster(Dictionary<string, double> Factors) : ISkillProfileQuotaModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SkillProfileQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSkillProfileQuotaAdjuster(double Minimum) : ISkillProfileQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the skill profile quota steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileQuotaEngine(List<ISkillProfileQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileQuotaEngine DefaultSkillProfileQuotaEngine()
    {
        return new SkillProfileQuotaEngine(
            new List<ISkillProfileQuotaModifier>
            {
                new TieredSkillProfileQuotaAdjuster(
                    new List<double> { 2880.0, 4595.0 },
                    new List<double> { 0.84, 0.97 }),
                new LabelSkillProfileQuotaAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.86, ["south"] = 1.25, ["retail"] = 1.11 }),
                new MinimumSkillProfileQuotaAdjuster(1.1),
            },
            new List<string>(),
            1.8,
            55836.9);
    }

    public double Evaluate(SkillProfileQuota item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
