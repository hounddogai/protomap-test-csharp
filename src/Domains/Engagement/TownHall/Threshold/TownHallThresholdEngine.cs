namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall threshold calculation.</summary>
public interface ITownHallThresholdModifier
{
    string Describe();
    double Adjust(TownHallThreshold item, double valueValue);
}

public sealed record MinimumTownHallThresholdAdjuster(double Minimum) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTownHallThresholdAdjuster(double Percent) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTownHallThresholdAdjuster(double Cap) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleTownHallThresholdAdjuster(Dictionary<string, double> Factors) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTownHallThresholdAdjuster(double Step) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTownHallThresholdAdjuster(List<double> Bounds, List<double> Rates) : ITownHallThresholdModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallThreshold item, double valueValue)
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

/// <summary>Runs the town hall threshold steps in order and keeps a trace of each.</summary>
public sealed class TownHallThresholdEngine(List<ITownHallThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallThresholdEngine DefaultTownHallThresholdEngine()
    {
        return new TownHallThresholdEngine(
            new List<ITownHallThresholdModifier>
            {
                new MinimumTownHallThresholdAdjuster(47.7),
                new PercentTownHallThresholdAdjuster(-1.9),
                new CapTownHallThresholdAdjuster(44926.0),
                new TitleTownHallThresholdAdjuster(new Dictionary<string, double> { ["retail"] = 1.19 }),
                new RoundingTownHallThresholdAdjuster(0.05),
                new TieredTownHallThresholdAdjuster(
                    new List<double> { 22.0, 7986.0 },
                    new List<double> { 1.04, 0.94 }),
            },
            new List<string>(),
            0.4,
            3407.0);
    }

    public double Evaluate(TownHallThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
