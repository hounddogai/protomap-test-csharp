namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pulse survey override calculation.</summary>
public interface IDefaultPulseSurveyOverrideModifier
{
    string Describe();
    double Adjust(DefaultPulseSurveyOverride item, double valueValue);
}

public sealed record FlaggedDefaultPulseSurveyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPulseSurveyOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPulseSurveyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultPulseSurveyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPulseSurveyOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPulseSurveyOverride item, double valueValue)
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

public sealed record RoundingDefaultPulseSurveyOverrideAdjuster(double Step) : IDefaultPulseSurveyOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPulseSurveyOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultPulseSurveyOverrideCreatedAtOffsetAdjuster(double Weight) : IDefaultPulseSurveyOverrideModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(DefaultPulseSurveyOverride item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the default pulse survey override steps in order and keeps a trace of each.</summary>
public sealed class DefaultPulseSurveyOverrideEngine(List<IDefaultPulseSurveyOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPulseSurveyOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPulseSurveyOverrideEngine DefaultDefaultPulseSurveyOverrideEngine(Registry flags)
    {
        return new DefaultPulseSurveyOverrideEngine(
            new List<IDefaultPulseSurveyOverrideModifier>
            {
                new FlaggedDefaultPulseSurveyOverrideAdjuster(flags, "default-pulse-survey-override-strict", -0.9),
                new TieredDefaultPulseSurveyOverrideAdjuster(
                    new List<double> { 3066.0, 8616.0, 9826.0 },
                    new List<double> { 0.91, 1.0, 0.9 }),
                new RoundingDefaultPulseSurveyOverrideAdjuster(0.5),
                new DefaultPulseSurveyOverrideCreatedAtOffsetAdjuster(1.83),
            },
            new List<string>(),
            4.2,
            31660.7);
    }

    public double Price(DefaultPulseSurveyOverride item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPulseSurveyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
