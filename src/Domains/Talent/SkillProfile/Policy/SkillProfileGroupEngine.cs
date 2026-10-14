namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile group calculation.</summary>
public interface ISkillProfileGroupAdjuster
{
    string Describe();
    double Adjust(SkillProfileGroup item, double valueValue);
}

public sealed record CapSkillProfileGroupAdjuster(double Cap) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSkillProfileGroupAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSkillProfileGroupAdjuster(double Step) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSkillProfileGroupAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
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

public sealed record SkillProfileGroupLimitOffsetAdjuster(double Weight) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record NameSkillProfileGroupAdjuster(Dictionary<string, double> Factors) : ISkillProfileGroupAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SkillProfileGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the skill profile group steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileGroupEngine(List<ISkillProfileGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileGroupEngine DefaultSkillProfileGroupEngine(Registry flags)
    {
        return new SkillProfileGroupEngine(
            new List<ISkillProfileGroupAdjuster>
            {
                new CapSkillProfileGroupAdjuster(22992.0),
                new FlaggedSkillProfileGroupAdjuster(flags, "skill-profile-group-manual", -8.4),
                new RoundingSkillProfileGroupAdjuster(1.0),
                new TieredSkillProfileGroupAdjuster(
                    new List<double> { 3331.0, 9154.0 },
                    new List<double> { 0.95, 1.08 }),
                new SkillProfileGroupLimitOffsetAdjuster(0.62),
                new NameSkillProfileGroupAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.75, ["west"] = 0.85 }),
            },
            new List<string>(),
            1.1,
            42470.3);
    }

    public double Price(SkillProfileGroup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
