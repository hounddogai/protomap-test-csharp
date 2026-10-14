namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional exception preference calculation.</summary>
public interface IRegionalExceptionPreferenceModifier
{
    string Describe();
    double Adjust(RegionalExceptionPreference item, double valueValue);
}

public sealed record PercentRegionalExceptionPreferenceAdjuster(double Percent) : IRegionalExceptionPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalExceptionPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalExceptionPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IRegionalExceptionPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalExceptionPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleRegionalExceptionPreferenceAdjuster(Dictionary<string, double> Factors) : IRegionalExceptionPreferenceModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalExceptionPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalExceptionPreferenceAdjuster(double Step) : IRegionalExceptionPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalExceptionPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional exception preference steps in order and keeps a trace of each.</summary>
public sealed class RegionalExceptionPreferenceEngine(List<IRegionalExceptionPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalExceptionPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalExceptionPreferenceEngine DefaultRegionalExceptionPreferenceEngine(Registry flags)
    {
        return new RegionalExceptionPreferenceEngine(
            new List<IRegionalExceptionPreferenceModifier>
            {
                new PercentRegionalExceptionPreferenceAdjuster(-18.9),
                new FlaggedRegionalExceptionPreferenceAdjuster(flags, "regional-exception-preference-tiered", -2.1),
                new LocaleRegionalExceptionPreferenceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.06, ["east"] = 0.95 }),
                new RoundingRegionalExceptionPreferenceAdjuster(1.0),
            },
            new List<string>(),
            2.1,
            95785.6);
    }

    public double Run(RegionalExceptionPreference item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalExceptionPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
