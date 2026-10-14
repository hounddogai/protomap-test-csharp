namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall baseline calculation.</summary>
public interface ITownHallBaselineModifier
{
    string Describe();
    double Adjust(TownHallBaseline item, double valueValue);
}

public sealed record PercentTownHallBaselineAdjuster(double Percent) : ITownHallBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTownHallBaselineAdjuster(double Step) : ITownHallBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTownHallBaselineAdjuster(double Minimum) : ITownHallBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTownHallBaselineAdjuster(List<double> Bounds, List<double> Rates) : ITownHallBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallBaseline item, double valueValue)
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

public sealed record SourceTownHallBaselineAdjuster(Dictionary<string, double> Factors) : ITownHallBaselineModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TownHallBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the town hall baseline steps in order and keeps a trace of each.</summary>
public sealed class TownHallBaselineEngine(List<ITownHallBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallBaselineEngine DefaultTownHallBaselineEngine()
    {
        return new TownHallBaselineEngine(
            new List<ITownHallBaselineModifier>
            {
                new PercentTownHallBaselineAdjuster(-16.2),
                new RoundingTownHallBaselineAdjuster(5.0),
                new MinimumTownHallBaselineAdjuster(41.9),
                new TieredTownHallBaselineAdjuster(
                    new List<double> { 193.0, 3773.0, 5872.0, 8714.0 },
                    new List<double> { 1.09, 1.05, 1.06, 1.15 }),
                new SourceTownHallBaselineAdjuster(new Dictionary<string, double> { ["west"] = 0.92 }),
            },
            new List<string>(),
            4.1,
            26282.5);
    }

    public double Compute(TownHallBaseline item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
