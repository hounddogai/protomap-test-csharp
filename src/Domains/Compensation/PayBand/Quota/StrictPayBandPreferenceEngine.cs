namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pay band preference calculation.</summary>
public interface IStrictPayBandPreferenceModifier
{
    string Describe();
    double Adjust(StrictPayBandPreference item, double valueValue);
}

public sealed record PercentStrictPayBandPreferenceAdjuster(double Percent) : IStrictPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPayBandPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictPayBandPreferenceAdjuster(double Minimum) : IStrictPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPayBandPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictPayBandPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IStrictPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPayBandPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStrictPayBandPreferenceAdjuster(Dictionary<string, double> Factors) : IStrictPayBandPreferenceModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictPayBandPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingStrictPayBandPreferenceAdjuster(double Step) : IStrictPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPayBandPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict pay band preference steps in order and keeps a trace of each.</summary>
public sealed class StrictPayBandPreferenceEngine(List<IStrictPayBandPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPayBandPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPayBandPreferenceEngine DefaultStrictPayBandPreferenceEngine(Registry flags)
    {
        return new StrictPayBandPreferenceEngine(
            new List<IStrictPayBandPreferenceModifier>
            {
                new PercentStrictPayBandPreferenceAdjuster(8.5),
                new MinimumStrictPayBandPreferenceAdjuster(41.5),
                new FlaggedStrictPayBandPreferenceAdjuster(flags, "strict-pay-band-preference-scheduled", 3.4),
                new NameStrictPayBandPreferenceAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.18, ["west"] = 1.38 }),
                new RoundingStrictPayBandPreferenceAdjuster(0.5),
            },
            new List<string>(),
            1.5,
            60264.7);
    }

    public double Evaluate(StrictPayBandPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictPayBandPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
