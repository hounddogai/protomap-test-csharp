namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant preference calculation.</summary>
public interface IEquityGrantPreferenceModifier
{
    string Describe();
    double Adjust(EquityGrantPreference item, double valueValue);
}

public sealed record PercentEquityGrantPreferenceAdjuster(double Percent) : IEquityGrantPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEquityGrantPreferenceAdjuster(double Cap) : IEquityGrantPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EquityGrantPreferenceVersionOffsetAdjuster(double Weight) : IEquityGrantPreferenceModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(EquityGrantPreference item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record FlaggedEquityGrantPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelEquityGrantPreferenceAdjuster(Dictionary<string, double> Factors) : IEquityGrantPreferenceModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EquityGrantPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the equity grant preference steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantPreferenceEngine(List<IEquityGrantPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantPreferenceEngine DefaultEquityGrantPreferenceEngine(Registry flags)
    {
        return new EquityGrantPreferenceEngine(
            new List<IEquityGrantPreferenceModifier>
            {
                new PercentEquityGrantPreferenceAdjuster(-9.3),
                new CapEquityGrantPreferenceAdjuster(8164.0),
                new EquityGrantPreferenceVersionOffsetAdjuster(0.49),
                new FlaggedEquityGrantPreferenceAdjuster(flags, "equity-grant-preference-lenient", 1.5),
                new LabelEquityGrantPreferenceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.25, ["south"] = 0.93 }),
            },
            new List<string>(),
            1.5,
            73579.6);
    }

    public double Compute(EquityGrantPreference item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
