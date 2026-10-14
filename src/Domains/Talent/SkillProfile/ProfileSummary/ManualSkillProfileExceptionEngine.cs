namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual skill profile exception calculation.</summary>
public interface IManualSkillProfileExceptionModifier
{
    string Describe();
    double Adjust(ManualSkillProfileException item, double valueValue);
}

public sealed record MinimumManualSkillProfileExceptionAdjuster(double Minimum) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredManualSkillProfileExceptionAdjuster(List<double> Bounds, List<double> Rates) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
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

public sealed record RoundingManualSkillProfileExceptionAdjuster(double Step) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualSkillProfileExceptionAdjuster(Registry Flags, string Flag, double Percent) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryManualSkillProfileExceptionAdjuster(Dictionary<string, double> Factors) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentManualSkillProfileExceptionAdjuster(double Percent) : IManualSkillProfileExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSkillProfileException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual skill profile exception steps in order and keeps a trace of each.</summary>
public sealed class ManualSkillProfileExceptionEngine(List<IManualSkillProfileExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSkillProfileExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSkillProfileExceptionEngine DefaultManualSkillProfileExceptionEngine(Registry flags)
    {
        return new ManualSkillProfileExceptionEngine(
            new List<IManualSkillProfileExceptionModifier>
            {
                new MinimumManualSkillProfileExceptionAdjuster(43.7),
                new TieredManualSkillProfileExceptionAdjuster(
                    new List<double> { 391.0, 601.0, 2410.0 },
                    new List<double> { 0.9, 0.87, 1.16 }),
                new RoundingManualSkillProfileExceptionAdjuster(0.1),
                new FlaggedManualSkillProfileExceptionAdjuster(
                    flags,
                    "manual-skill-profile-exception-adaptive",
                    -0.5),
                new CategoryManualSkillProfileExceptionAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.96, ["east"] = 0.99 }),
                new PercentManualSkillProfileExceptionAdjuster(-6.8),
            },
            new List<string>(),
            4.9,
            53577.4);
    }

    public double Evaluate(ManualSkillProfileException item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSkillProfileException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
