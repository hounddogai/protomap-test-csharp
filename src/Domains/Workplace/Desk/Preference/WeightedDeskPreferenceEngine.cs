namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted desk preference calculation.</summary>
public interface IWeightedDeskPreferenceFactor
{
    string Describe();
    double Adjust(WeightedDeskPreference item, double valueValue);
}

public sealed record MinimumWeightedDeskPreferenceAdjuster(double Minimum) : IWeightedDeskPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedDeskPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionWeightedDeskPreferenceAdjuster(Dictionary<string, double> Factors) : IWeightedDeskPreferenceFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(WeightedDeskPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedDeskPreferenceAdjuster(double Cap) : IWeightedDeskPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedDeskPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWeightedDeskPreferenceAdjuster(double Step) : IWeightedDeskPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedDeskPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedDeskPreferenceTenureMonthsOffsetAdjuster(double Weight) : IWeightedDeskPreferenceFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(WeightedDeskPreference item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the weighted desk preference steps in order and keeps a trace of each.</summary>
public sealed class WeightedDeskPreferenceEngine(List<IWeightedDeskPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedDeskPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedDeskPreferenceEngine DefaultWeightedDeskPreferenceEngine()
    {
        return new WeightedDeskPreferenceEngine(
            new List<IWeightedDeskPreferenceFactor>
            {
                new MinimumWeightedDeskPreferenceAdjuster(34.6),
                new RegionWeightedDeskPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["central"] = 1.18, ["south"] = 1.23 }),
                new CapWeightedDeskPreferenceAdjuster(3431.0),
                new RoundingWeightedDeskPreferenceAdjuster(0.05),
                new WeightedDeskPreferenceTenureMonthsOffsetAdjuster(1.03),
            },
            new List<string>(),
            2.2,
            56355.0);
    }

    public double Compute(WeightedDeskPreference item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedDeskPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
