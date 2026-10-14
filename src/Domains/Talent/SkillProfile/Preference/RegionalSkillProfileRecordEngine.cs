namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional skill profile record calculation.</summary>
public interface IRegionalSkillProfileRecordModifier
{
    string Describe();
    double Adjust(RegionalSkillProfileRecord item, double valueValue);
}

public sealed record MinimumRegionalSkillProfileRecordAdjuster(double Minimum) : IRegionalSkillProfileRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSkillProfileRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalSkillProfileRecordAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSkillProfileRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSkillProfileRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalSkillProfileRecordAdjuster(double Percent) : IRegionalSkillProfileRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSkillProfileRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalSkillProfileRecordAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSkillProfileRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSkillProfileRecord item, double valueValue)
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

/// <summary>Runs the regional skill profile record steps in order and keeps a trace of each.</summary>
public sealed class RegionalSkillProfileRecordEngine(List<IRegionalSkillProfileRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSkillProfileRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSkillProfileRecordEngine DefaultRegionalSkillProfileRecordEngine(Registry flags)
    {
        return new RegionalSkillProfileRecordEngine(
            new List<IRegionalSkillProfileRecordModifier>
            {
                new MinimumRegionalSkillProfileRecordAdjuster(6.3),
                new FlaggedRegionalSkillProfileRecordAdjuster(flags, "regional-skill-profile-record-legacy", 9.5),
                new PercentRegionalSkillProfileRecordAdjuster(0.7),
                new TieredRegionalSkillProfileRecordAdjuster(
                    new List<double> { 387.0, 8730.0 },
                    new List<double> { 0.89, 0.83 }),
            },
            new List<string>(),
            3.9,
            50721.3);
    }

    public double Compute(RegionalSkillProfileRecord item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSkillProfileRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
