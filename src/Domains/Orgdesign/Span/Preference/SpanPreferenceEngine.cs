namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span preference calculation.</summary>
public interface ISpanPreferenceFactor
{
    string Describe();
    double Adjust(SpanPreference item, double valueValue);
}

public sealed record PercentSpanPreferenceAdjuster(double Percent) : ISpanPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanPreferenceAdjuster(double Minimum) : ISpanPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SpanPreferenceScoreOffsetAdjuster(double Weight) : ISpanPreferenceFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(SpanPreference item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedSpanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISpanPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSpanPreferenceAdjuster(double Step) : ISpanPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the span preference steps in order and keeps a trace of each.</summary>
public sealed class SpanPreferenceEngine(List<ISpanPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanPreferenceEngine DefaultSpanPreferenceEngine(Registry flags)
    {
        return new SpanPreferenceEngine(
            new List<ISpanPreferenceFactor>
            {
                new PercentSpanPreferenceAdjuster(-13.5),
                new MinimumSpanPreferenceAdjuster(12.1),
                new SpanPreferenceScoreOffsetAdjuster(0.21),
                new FlaggedSpanPreferenceAdjuster(flags, "span-preference-batched", -0.4),
                new RoundingSpanPreferenceAdjuster(0.05),
            },
            new List<string>(),
            2.3,
            19529.0);
    }

    public double Run(SpanPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
