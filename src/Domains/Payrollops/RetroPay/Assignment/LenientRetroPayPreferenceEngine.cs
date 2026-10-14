namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient retro pay preference calculation.</summary>
public interface ILenientRetroPayPreferenceModifier
{
    string Describe();
    double Adjust(LenientRetroPayPreference item, double valueValue);
}

public sealed record CapLenientRetroPayPreferenceAdjuster(double Cap) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientRetroPayPreferenceAdjuster(double Step) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LenientRetroPayPreferenceSequenceOffsetAdjuster(double Weight) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record MinimumLenientRetroPayPreferenceAdjuster(double Minimum) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleLenientRetroPayPreferenceAdjuster(Dictionary<string, double> Factors) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientRetroPayPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ILenientRetroPayPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRetroPayPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient retro pay preference steps in order and keeps a trace of each.</summary>
public sealed class LenientRetroPayPreferenceEngine(List<ILenientRetroPayPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRetroPayPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRetroPayPreferenceEngine DefaultLenientRetroPayPreferenceEngine(Registry flags)
    {
        return new LenientRetroPayPreferenceEngine(
            new List<ILenientRetroPayPreferenceModifier>
            {
                new CapLenientRetroPayPreferenceAdjuster(14623.0),
                new RoundingLenientRetroPayPreferenceAdjuster(0.05),
                new LenientRetroPayPreferenceSequenceOffsetAdjuster(0.64),
                new MinimumLenientRetroPayPreferenceAdjuster(14.8),
                new TitleLenientRetroPayPreferenceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.33, ["east"] = 0.99 }),
                new FlaggedLenientRetroPayPreferenceAdjuster(flags, "lenient-retro-pay-preference-legacy", 8.8),
            },
            new List<string>(),
            0.8,
            40183.7);
    }

    public double Run(LenientRetroPayPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRetroPayPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
