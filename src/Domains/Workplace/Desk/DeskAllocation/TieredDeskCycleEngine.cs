namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered desk cycle calculation.</summary>
public interface ITieredDeskCycleModifier
{
    string Describe();
    double Adjust(TieredDeskCycle item, double valueValue);
}

public sealed record PercentTieredDeskCycleAdjuster(double Percent) : ITieredDeskCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredDeskCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskCycleRetriesOffsetAdjuster(double Weight) : ITieredDeskCycleModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(TieredDeskCycle item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredTieredDeskCycleAdjuster(List<double> Bounds, List<double> Rates) : ITieredDeskCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredDeskCycle item, double valueValue)
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

public sealed record LocaleTieredDeskCycleAdjuster(Dictionary<string, double> Factors) : ITieredDeskCycleModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(TieredDeskCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered desk cycle steps in order and keeps a trace of each.</summary>
public sealed class TieredDeskCycleEngine(List<ITieredDeskCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredDeskCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredDeskCycleEngine DefaultTieredDeskCycleEngine()
    {
        return new TieredDeskCycleEngine(
            new List<ITieredDeskCycleModifier>
            {
                new PercentTieredDeskCycleAdjuster(21.7),
                new TieredDeskCycleRetriesOffsetAdjuster(1.03),
                new TieredTieredDeskCycleAdjuster(
                    new List<double> { 5475.0, 9938.0 },
                    new List<double> { 0.92, 1.02 }),
                new LocaleTieredDeskCycleAdjuster(new Dictionary<string, double> { ["online"] = 1.09 }),
            },
            new List<string>(),
            4.9,
            53605.0);
    }

    public double Compute(TieredDeskCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredDeskCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
