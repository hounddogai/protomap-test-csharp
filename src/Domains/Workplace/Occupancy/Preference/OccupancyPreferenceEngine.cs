namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy preference calculation.</summary>
public interface IOccupancyPreferenceStep
{
    string Describe();
    double Adjust(OccupancyPreference item, double valueValue);
}

public sealed record MinimumOccupancyPreferenceAdjuster(double Minimum) : IOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryOccupancyPreferenceAdjuster(Dictionary<string, double> Factors) : IOccupancyPreferenceStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(OccupancyPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedOccupancyPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredOccupancyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyPreference item, double valueValue)
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

public sealed record RoundingOccupancyPreferenceAdjuster(double Step) : IOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the occupancy preference steps in order and keeps a trace of each.</summary>
public sealed class OccupancyPreferenceEngine(List<IOccupancyPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyPreferenceEngine DefaultOccupancyPreferenceEngine(Registry flags)
    {
        return new OccupancyPreferenceEngine(
            new List<IOccupancyPreferenceStep>
            {
                new MinimumOccupancyPreferenceAdjuster(4.5),
                new CountryOccupancyPreferenceAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.22, ["south"] = 1.39 }),
                new FlaggedOccupancyPreferenceAdjuster(flags, "occupancy-preference-seasonal", 6.8),
                new TieredOccupancyPreferenceAdjuster(
                    new List<double> { 1431.0, 9197.0 },
                    new List<double> { 0.91, 1.01 }),
                new RoundingOccupancyPreferenceAdjuster(0.1),
            },
            new List<string>(),
            4.7,
            70014.2);
    }

    public double Evaluate(OccupancyPreference item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
