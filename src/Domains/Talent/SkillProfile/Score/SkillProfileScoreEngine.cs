namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile score calculation.</summary>
public interface ISkillProfileScoreFactor
{
    string Describe();
    double Adjust(SkillProfileScore item, double valueValue);
}

public sealed record SkillProfileScoreWeightOffsetAdjuster(double Weight) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingSkillProfileScoreAdjuster(double Step) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileScoreAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileScoreAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
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

public sealed record PercentSkillProfileScoreAdjuster(double Percent) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSkillProfileScoreAdjuster(double Minimum) : ISkillProfileScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the skill profile score steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileScoreEngine(List<ISkillProfileScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileScoreEngine DefaultSkillProfileScoreEngine(Registry flags)
    {
        return new SkillProfileScoreEngine(
            new List<ISkillProfileScoreFactor>
            {
                new SkillProfileScoreWeightOffsetAdjuster(0.78),
                new RoundingSkillProfileScoreAdjuster(0.25),
                new FlaggedSkillProfileScoreAdjuster(flags, "skill-profile-score-strict", -5.8),
                new TieredSkillProfileScoreAdjuster(
                    new List<double> { 381.0, 1674.0, 2194.0, 7547.0 },
                    new List<double> { 0.94, 0.9, 1.09, 1.12 }),
                new PercentSkillProfileScoreAdjuster(5.3),
                new MinimumSkillProfileScoreAdjuster(26.8),
            },
            new List<string>(),
            3.0,
            70734.5);
    }

    public double Evaluate(SkillProfileScore item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
