namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall preference calculation.</summary>
public interface ITownHallPreferenceFactor
{
    string Describe();
    double Adjust(TownHallPreference item, double valueValue);
}

public sealed record TieredTownHallPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ITownHallPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallPreference item, double valueValue)
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

public sealed record CapTownHallPreferenceAdjuster(double Cap) : ITownHallPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTownHallPreferenceAdjuster(double Percent) : ITownHallPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTownHallPreferenceAdjuster(double Minimum) : ITownHallPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the town hall preference steps in order and keeps a trace of each.</summary>
public sealed class TownHallPreferenceEngine(List<ITownHallPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallPreferenceEngine DefaultTownHallPreferenceEngine()
    {
        return new TownHallPreferenceEngine(
            new List<ITownHallPreferenceFactor>
            {
                new TieredTownHallPreferenceAdjuster(
                    new List<double> { 4026.0, 8168.0 },
                    new List<double> { 1.03, 1.11 }),
                new CapTownHallPreferenceAdjuster(47494.0),
                new PercentTownHallPreferenceAdjuster(21.1),
                new MinimumTownHallPreferenceAdjuster(27.9),
            },
            new List<string>(),
            2.9,
            99068.4);
    }

    public double Compute(TownHallPreference item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
