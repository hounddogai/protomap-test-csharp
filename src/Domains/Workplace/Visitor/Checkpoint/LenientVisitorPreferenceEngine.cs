namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient visitor preference calculation.</summary>
public interface ILenientVisitorPreferenceModifier
{
    string Describe();
    double Adjust(LenientVisitorPreference item, double valueValue);
}

public sealed record MinimumLenientVisitorPreferenceAdjuster(double Minimum) : ILenientVisitorPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientVisitorPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientVisitorPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ILenientVisitorPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientVisitorPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientVisitorPreferenceAdjuster(double Percent) : ILenientVisitorPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientVisitorPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientVisitorPreferenceAdjuster(double Cap) : ILenientVisitorPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientVisitorPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient visitor preference steps in order and keeps a trace of each.</summary>
public sealed class LenientVisitorPreferenceEngine(List<ILenientVisitorPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVisitorPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVisitorPreferenceEngine DefaultLenientVisitorPreferenceEngine(Registry flags)
    {
        return new LenientVisitorPreferenceEngine(
            new List<ILenientVisitorPreferenceModifier>
            {
                new MinimumLenientVisitorPreferenceAdjuster(4.5),
                new FlaggedLenientVisitorPreferenceAdjuster(flags, "lenient-visitor-preference-fallback", -5.6),
                new PercentLenientVisitorPreferenceAdjuster(-1.0),
                new CapLenientVisitorPreferenceAdjuster(870.0),
            },
            new List<string>(),
            2.4,
            58210.4);
    }

    public double Compute(LenientVisitorPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientVisitorPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
