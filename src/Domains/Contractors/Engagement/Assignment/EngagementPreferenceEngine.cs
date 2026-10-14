namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement preference calculation.</summary>
public interface IEngagementPreferenceStep
{
    string Describe();
    double Adjust(EngagementPreference item, double valueValue);
}

public sealed record TieredEngagementPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IEngagementPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementPreference item, double valueValue)
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

public sealed record DepartmentEngagementPreferenceAdjuster(Dictionary<string, double> Factors) : IEngagementPreferenceStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(EngagementPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record EngagementPreferenceRateOffsetAdjuster(double Weight) : IEngagementPreferenceStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(EngagementPreference item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedEngagementPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IEngagementPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEngagementPreferenceAdjuster(double Step) : IEngagementPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the engagement preference steps in order and keeps a trace of each.</summary>
public sealed class EngagementPreferenceEngine(List<IEngagementPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementPreferenceEngine DefaultEngagementPreferenceEngine(Registry flags)
    {
        return new EngagementPreferenceEngine(
            new List<IEngagementPreferenceStep>
            {
                new TieredEngagementPreferenceAdjuster(
                    new List<double> { 1833.0, 2482.0, 4734.0 },
                    new List<double> { 1.11, 1.08, 0.98 }),
                new DepartmentEngagementPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.17, ["retail"] = 0.72, ["south"] = 0.74 }),
                new EngagementPreferenceRateOffsetAdjuster(0.54),
                new FlaggedEngagementPreferenceAdjuster(flags, "engagement-preference-scheduled", -6.9),
                new RoundingEngagementPreferenceAdjuster(0.1),
            },
            new List<string>(),
            1.1,
            65293.3);
    }

    public double Evaluate(EngagementPreference item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
