namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default skill profile draft calculation.</summary>
public interface IDefaultSkillProfileDraftStep
{
    string Describe();
    double Adjust(DefaultSkillProfileDraft item, double valueValue);
}

public sealed record PercentDefaultSkillProfileDraftAdjuster(double Percent) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultSkillProfileDraftAdjuster(List<double> Bounds, List<double> Rates) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
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

public sealed record DepartmentDefaultSkillProfileDraftAdjuster(Dictionary<string, double> Factors) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultSkillProfileDraftAdjuster(double Cap) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDefaultSkillProfileDraftAdjuster(double Step) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultSkillProfileDraftAdjuster(Registry Flags, string Flag, double Percent) : IDefaultSkillProfileDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultSkillProfileDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default skill profile draft steps in order and keeps a trace of each.</summary>
public sealed class DefaultSkillProfileDraftEngine(List<IDefaultSkillProfileDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSkillProfileDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSkillProfileDraftEngine DefaultDefaultSkillProfileDraftEngine(Registry flags)
    {
        return new DefaultSkillProfileDraftEngine(
            new List<IDefaultSkillProfileDraftStep>
            {
                new PercentDefaultSkillProfileDraftAdjuster(2.2),
                new TieredDefaultSkillProfileDraftAdjuster(
                    new List<double> { 1596.0, 6723.0, 8092.0, 8746.0 },
                    new List<double> { 0.84, 1.06, 0.96, 0.85 }),
                new DepartmentDefaultSkillProfileDraftAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.15, ["south"] = 0.98 }),
                new CapDefaultSkillProfileDraftAdjuster(8302.0),
                new RoundingDefaultSkillProfileDraftAdjuster(0.1),
                new FlaggedDefaultSkillProfileDraftAdjuster(flags, "default-skill-profile-draft-primary", 0.7),
            },
            new List<string>(),
            0.2,
            42824.1);
    }

    public double Evaluate(DefaultSkillProfileDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSkillProfileDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
