namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal skill profile preference calculation.</summary>
public interface ISeasonalSkillProfilePreferenceModifier
{
    string Describe();
    double Adjust(SeasonalSkillProfilePreference item, double valueValue);
}

public sealed record RoundingSeasonalSkillProfilePreferenceAdjuster(double Step) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSeasonalSkillProfilePreferenceAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
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

public sealed record CapSeasonalSkillProfilePreferenceAdjuster(double Cap) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalSkillProfilePreferenceAdjuster(double Minimum) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalSkillProfilePreferenceAdjuster(double Percent) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalSkillProfilePreferenceBudgetOffsetAdjuster(double Weight) : ISeasonalSkillProfilePreferenceModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(SeasonalSkillProfilePreference item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the seasonal skill profile preference steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSkillProfilePreferenceEngine(List<ISeasonalSkillProfilePreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSkillProfilePreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSkillProfilePreferenceEngine DefaultSeasonalSkillProfilePreferenceEngine()
    {
        return new SeasonalSkillProfilePreferenceEngine(
            new List<ISeasonalSkillProfilePreferenceModifier>
            {
                new RoundingSeasonalSkillProfilePreferenceAdjuster(5.0),
                new TieredSeasonalSkillProfilePreferenceAdjuster(
                    new List<double> { 1588.0, 2587.0, 2617.0, 9185.0 },
                    new List<double> { 1.12, 1.16, 0.82, 0.9 }),
                new CapSeasonalSkillProfilePreferenceAdjuster(49697.0),
                new MinimumSeasonalSkillProfilePreferenceAdjuster(27.7),
                new PercentSeasonalSkillProfilePreferenceAdjuster(-20.0),
                new SeasonalSkillProfilePreferenceBudgetOffsetAdjuster(0.73),
            },
            new List<string>(),
            2.2,
            87770.6);
    }

    public double Evaluate(SeasonalSkillProfilePreference item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSkillProfilePreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
