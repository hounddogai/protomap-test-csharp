namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile request calculation.</summary>
public interface ISkillProfileRequestFactor
{
    string Describe();
    double Adjust(SkillProfileRequest item, double valueValue);
}

public sealed record MinimumSkillProfileRequestAdjuster(double Minimum) : ISkillProfileRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSkillProfileRequestAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileRequestFloorValueOffsetAdjuster(double Weight) : ISkillProfileRequestFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SkillProfileRequest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapSkillProfileRequestAdjuster(double Cap) : ISkillProfileRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSkillProfileRequestAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileRequest item, double valueValue)
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

/// <summary>Runs the skill profile request steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileRequestEngine(List<ISkillProfileRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileRequestEngine DefaultSkillProfileRequestEngine(Registry flags)
    {
        return new SkillProfileRequestEngine(
            new List<ISkillProfileRequestFactor>
            {
                new MinimumSkillProfileRequestAdjuster(49.2),
                new FlaggedSkillProfileRequestAdjuster(flags, "skill-profile-request-capped", -3.3),
                new SkillProfileRequestFloorValueOffsetAdjuster(1.34),
                new CapSkillProfileRequestAdjuster(14375.0),
                new TieredSkillProfileRequestAdjuster(
                    new List<double> { 4635.0, 9162.0 },
                    new List<double> { 0.91, 1.01 }),
            },
            new List<string>(),
            4.9,
            25258.1);
    }

    public double Compute(SkillProfileRequest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
