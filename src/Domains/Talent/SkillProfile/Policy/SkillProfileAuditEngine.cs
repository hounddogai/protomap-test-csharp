namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile audit calculation.</summary>
public interface ISkillProfileAuditStep
{
    string Describe();
    double Adjust(SkillProfileAudit item, double valueValue);
}

public sealed record SkillProfileAuditMarginOffsetAdjuster(double Weight) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumSkillProfileAuditAdjuster(double Minimum) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSkillProfileAuditAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSkillProfileAuditAdjuster(double Step) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSkillProfileAuditAdjuster(double Cap) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSkillProfileAuditAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileAudit item, double valueValue)
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

/// <summary>Runs the skill profile audit steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileAuditEngine(List<ISkillProfileAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileAuditEngine DefaultSkillProfileAuditEngine(Registry flags)
    {
        return new SkillProfileAuditEngine(
            new List<ISkillProfileAuditStep>
            {
                new SkillProfileAuditMarginOffsetAdjuster(1.56),
                new MinimumSkillProfileAuditAdjuster(30.0),
                new FlaggedSkillProfileAuditAdjuster(flags, "skill-profile-audit-global", -4.2),
                new RoundingSkillProfileAuditAdjuster(1.0),
                new CapSkillProfileAuditAdjuster(36841.0),
                new TieredSkillProfileAuditAdjuster(
                    new List<double> { 3762.0, 5497.0, 7617.0, 9912.0 },
                    new List<double> { 0.82, 0.81, 0.92, 1.12 }),
            },
            new List<string>(),
            1.8,
            58270.1);
    }

    public double Evaluate(SkillProfileAudit item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
