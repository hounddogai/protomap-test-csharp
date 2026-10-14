namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal skill profile cycle calculation.</summary>
public interface ISeasonalSkillProfileCycleModifier
{
    string Describe();
    double Adjust(SeasonalSkillProfileCycle item, double valueValue);
}

public sealed record LabelSeasonalSkillProfileCycleAdjuster(Dictionary<string, double> Factors) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalSkillProfileCycleAdjuster(double Step) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalSkillProfileCycleAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalSkillProfileCycleAdjuster(double Minimum) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SeasonalSkillProfileCycleBudgetOffsetAdjuster(double Weight) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapSeasonalSkillProfileCycleAdjuster(double Cap) : ISeasonalSkillProfileCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSkillProfileCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal skill profile cycle steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSkillProfileCycleEngine(List<ISeasonalSkillProfileCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSkillProfileCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSkillProfileCycleEngine DefaultSeasonalSkillProfileCycleEngine(Registry flags)
    {
        return new SeasonalSkillProfileCycleEngine(
            new List<ISeasonalSkillProfileCycleModifier>
            {
                new LabelSeasonalSkillProfileCycleAdjuster(new Dictionary<string, double> { ["east"] = 0.81 }),
                new RoundingSeasonalSkillProfileCycleAdjuster(0.5),
                new FlaggedSeasonalSkillProfileCycleAdjuster(flags, "seasonal-skill-profile-cycle-seasonal", 2.9),
                new MinimumSeasonalSkillProfileCycleAdjuster(17.1),
                new SeasonalSkillProfileCycleBudgetOffsetAdjuster(1.52),
                new CapSeasonalSkillProfileCycleAdjuster(43204.0),
            },
            new List<string>(),
            1.9,
            64578.7);
    }

    public double Compute(SeasonalSkillProfileCycle item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSkillProfileCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
