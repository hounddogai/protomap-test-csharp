namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted pay run preference calculation.</summary>
public interface IWeightedPayRunPreferenceFactor
{
    string Describe();
    double Adjust(WeightedPayRunPreference item, double valueValue);
}

public sealed record MinimumWeightedPayRunPreferenceAdjuster(double Minimum) : IWeightedPayRunPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedPayRunPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedPayRunPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IWeightedPayRunPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedPayRunPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedPayRunPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPayRunPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPayRunPreference item, double valueValue)
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

public sealed record PercentWeightedPayRunPreferenceAdjuster(double Percent) : IWeightedPayRunPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPayRunPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedPayRunPreferenceLimitOffsetAdjuster(double Weight) : IWeightedPayRunPreferenceFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(WeightedPayRunPreference item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the weighted pay run preference steps in order and keeps a trace of each.</summary>
public sealed class WeightedPayRunPreferenceEngine(List<IWeightedPayRunPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPayRunPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPayRunPreferenceEngine DefaultWeightedPayRunPreferenceEngine(Registry flags)
    {
        return new WeightedPayRunPreferenceEngine(
            new List<IWeightedPayRunPreferenceFactor>
            {
                new MinimumWeightedPayRunPreferenceAdjuster(19.6),
                new FlaggedWeightedPayRunPreferenceAdjuster(flags, "weighted-pay-run-preference-fallback", 6.7),
                new TieredWeightedPayRunPreferenceAdjuster(
                    new List<double> { 4728.0, 6418.0, 6498.0, 7258.0 },
                    new List<double> { 1.06, 1.17, 1.08, 1.0 }),
                new PercentWeightedPayRunPreferenceAdjuster(4.3),
                new WeightedPayRunPreferenceLimitOffsetAdjuster(0.15),
            },
            new List<string>(),
            1.6,
            19585.8);
    }

    public double Run(WeightedPayRunPreference item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPayRunPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
