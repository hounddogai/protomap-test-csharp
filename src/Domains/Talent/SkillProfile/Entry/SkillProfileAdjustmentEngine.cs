namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile adjustment calculation.</summary>
public interface ISkillProfileAdjustmentFactor
{
    string Describe();
    double Adjust(SkillProfileAdjustment item, double valueValue);
}

public sealed record TieredSkillProfileAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileAdjustmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileAdjustment item, double valueValue)
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

public sealed record CapSkillProfileAdjustmentAdjuster(double Cap) : ISkillProfileAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SkillProfileAdjustmentLimitOffsetAdjuster(double Weight) : ISkillProfileAdjustmentFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SkillProfileAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record RoundingSkillProfileAdjustmentAdjuster(double Step) : ISkillProfileAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the skill profile adjustment steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileAdjustmentEngine(List<ISkillProfileAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileAdjustmentEngine DefaultSkillProfileAdjustmentEngine(Registry flags)
    {
        return new SkillProfileAdjustmentEngine(
            new List<ISkillProfileAdjustmentFactor>
            {
                new TieredSkillProfileAdjustmentAdjuster(
                    new List<double> { 844.0, 4797.0, 7467.0, 9874.0 },
                    new List<double> { 0.96, 1.06, 0.93, 0.88 }),
                new CapSkillProfileAdjustmentAdjuster(35159.0),
                new SkillProfileAdjustmentLimitOffsetAdjuster(1.03),
                new RoundingSkillProfileAdjustmentAdjuster(0.05),
                new FlaggedSkillProfileAdjustmentAdjuster(flags, "skill-profile-adjustment-strict", 4.4),
            },
            new List<string>(),
            4.2,
            30451.6);
    }

    public double Price(SkillProfileAdjustment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
