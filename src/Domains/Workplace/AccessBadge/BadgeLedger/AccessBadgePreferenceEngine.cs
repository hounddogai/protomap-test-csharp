namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge preference calculation.</summary>
public interface IAccessBadgePreferenceAdjuster
{
    string Describe();
    double Adjust(AccessBadgePreference item, double valueValue);
}

public sealed record RoundingAccessBadgePreferenceAdjuster(double Step) : IAccessBadgePreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgePreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAccessBadgePreferenceAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgePreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgePreference item, double valueValue)
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

public sealed record CapAccessBadgePreferenceAdjuster(double Cap) : IAccessBadgePreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgePreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleAccessBadgePreferenceAdjuster(Dictionary<string, double> Factors) : IAccessBadgePreferenceAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AccessBadgePreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AccessBadgePreferenceThresholdOffsetAdjuster(double Weight) : IAccessBadgePreferenceAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(AccessBadgePreference item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the access badge preference steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgePreferenceEngine(List<IAccessBadgePreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgePreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgePreferenceEngine DefaultAccessBadgePreferenceEngine()
    {
        return new AccessBadgePreferenceEngine(
            new List<IAccessBadgePreferenceAdjuster>
            {
                new RoundingAccessBadgePreferenceAdjuster(1.0),
                new TieredAccessBadgePreferenceAdjuster(
                    new List<double> { 1951.0, 6823.0 },
                    new List<double> { 1.07, 0.93 }),
                new CapAccessBadgePreferenceAdjuster(27477.0),
                new TitleAccessBadgePreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.23, ["retail"] = 0.81 }),
                new AccessBadgePreferenceThresholdOffsetAdjuster(0.36),
            },
            new List<string>(),
            0.4,
            62498.0);
    }

    public double Evaluate(AccessBadgePreference item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgePreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
